using Application.DTOs;
using Application.DTOs.UserDTO;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Application.Paggings;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using static Domain.Base.BaseException;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }
        public async Task<PaginatedList<BookingDTO>> GetBookingsAsync(
            int pageIndex,
            int pageSize,
            Guid? customerId = null,
            Guid? artistId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            BookingStatus? status = null)
        {
            var query = _unitOfWork.GetRepository<Booking>()
                .Entities
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .ThenInclude(s => s.Artist)
                .Where(b =>
                    !b.IsDeleted &&
                    (customerId == null || b.Customer!.Id == customerId) &&
                    (artistId == null || b.Service!.ArtistID == artistId) &&
                    (!startDate.HasValue || b.StartTime >= startDate.Value) &&
                    (!endDate.HasValue || b.EndTime <= endDate.Value) &&
                    (!status.HasValue || b.Status == status.Value)
                )
                .OrderByDescending(b => b.BookingDate);

            var bookings = await _unitOfWork.GetRepository<Booking>().GetPagging(query, pageIndex, pageSize);
            return _mapper.Map<PaginatedList<BookingDTO>>(bookings);
        }

        public async Task<BookingDTO> CreateBookingAsync(BookingCreateDTO bookingDto)
        {
            var serviceRepo = _unitOfWork.GetRepository<Service>();
            var customerRepo = _unitOfWork.GetRepository<ApplicationUser>();
            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            var service = await serviceRepo.GetByIdAsync(bookingDto.ServiceID);
            var customer = await customerRepo.GetByIdAsync(bookingDto.CustomerID);

            if (service == null)
                throw new ArgumentException("Invalid service ID.");
            if (customer == null)
                throw new ArgumentException("Invalid customer ID.");

            if (bookingDto.StartTime >= bookingDto.EndTime)
                throw new ArgumentException("StartTime must be earlier than EndTime.");

            // Check for overlapping bookings for the same service
            var isOverlap = await bookingRepo.Entities.AnyAsync(b =>
                b.ServiceID == bookingDto.ServiceID &&
                (
                    (bookingDto.StartTime < b.EndTime && bookingDto.EndTime > b.StartTime)
                ));

            if (isOverlap)
                throw new InvalidOperationException("The selected time slot is already booked for this service.");

            var booking = _mapper.Map<Booking>(bookingDto);
            booking.Service = service;
            booking.Customer = customer;
            booking.BookingDate = DateTime.UtcNow;
            booking.TotalPrice = service.Price;
            booking.Status = BookingStatus.Pending;

            await bookingRepo.InsertAsync(booking);
            await _unitOfWork.SaveAsync();

            var bookingWithIncludes = await bookingRepo.Entities
                .Include(b => b.Service).ThenInclude(s => s.Artist)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            return _mapper.Map<BookingDTO>(bookingWithIncludes);
        }

        public async Task<List<BookingStatusSummaryDTO>> GetBookingStatusSummaryAsync()
        {
            var summary = await _unitOfWork.GetRepository<Booking>()
                .Entities
                .Where(b => !b.IsDeleted && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Canceled))
                .GroupBy(b => b.BookingDate.Date)
                .Select(g => new BookingStatusSummaryDTO
                {
                    Date = g.Key,
                    CompletedCount = g.Count(b => b.Status == BookingStatus.Completed),
                    CanceledCount = g.Count(b => b.Status == BookingStatus.Canceled)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            return summary;
        }



        public async Task<BookingDTO> BookingForCustomerAsync(BookingCreateDTO bookingDto)
        {
            var serviceRepo = _unitOfWork.GetRepository<Service>();
            var customerRepo = _unitOfWork.GetRepository<ApplicationUser>();
            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            bookingDto.CustomerID = _userService.GetCurrentUserId() ?? throw new AuthenticationException("Customer not found");

            var service = await serviceRepo.GetByIdAsync(bookingDto.ServiceID);
            var customer = await customerRepo.GetByIdAsync(bookingDto.CustomerID);

            if (service == null)
                throw new ArgumentException("Invalid service ID.");
            if (customer == null)
                throw new ArgumentException("Invalid customer ID.");

            if (bookingDto.StartTime >= bookingDto.EndTime)
                throw new ArgumentException("StartTime must be earlier than EndTime.");

            // Check for overlapping bookings for the same service
            var isOverlap = await bookingRepo.Entities.AnyAsync(b =>
                b.ServiceID == bookingDto.ServiceID &&
                (
                    (bookingDto.StartTime < b.EndTime && bookingDto.EndTime > b.StartTime)
                ));

            if (isOverlap)
                throw new InvalidOperationException("The selected time slot is already booked for this service.");

            var booking = _mapper.Map<Booking>(bookingDto);
            booking.Service = service;
            booking.Customer = customer;
            booking.BookingDate = DateTime.UtcNow;
            booking.TotalPrice = service.Price;
            booking.Status = BookingStatus.Pending;

            await bookingRepo.InsertAsync(booking);
            await _unitOfWork.SaveAsync();

            var bookingWithIncludes = await bookingRepo.Entities
                .Include(b => b.Service).ThenInclude(s => s.Artist)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            return _mapper.Map<BookingDTO>(bookingWithIncludes);
        }

        public async Task<BookingDTO> StatusBookingAsync(Guid bookingId, BookingStatus status)
        {
            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            var booking = await bookingRepo.Entities
                .Include(b => b.Service).ThenInclude(s => s.Artist)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

            var userId = _userService.GetCurrentUserId();

            if (booking == null)
                throw new ArgumentException("Booking not found.");

            if(status == BookingStatus.Confirmed)
            {
                if (booking.Status != BookingStatus.Pending)
                    throw new InvalidOperationException("Only pending bookings can be confirmed.");

                if (booking.Service!.ArtistID != userId)
                    throw new AuthenticationException("You are not authorized to confirm this booking.");
            }

            if (status == BookingStatus.Canceled)
            {
                if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                    throw new InvalidOperationException("Only pending or confirmed bookings can be canceled.");
                if (booking.CustomerID != userId || booking.Service!.ArtistID != userId)
                    throw new AuthenticationException("You are not authorized to cancel this booking.");
            }

            booking.Status = status;
            await _unitOfWork.SaveAsync();

            return _mapper.Map<BookingDTO>(booking);
        }

    }
}
