using Application.DTOs;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Application.Paggings;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            DateTime? startDate = null,
            DateTime? endDate = null,
            BookingStatus? status = null)
        {
            var query = _unitOfWork.GetRepository<Booking>()
                .Entities
                .Include(b => b.Customer)
                .Include(b => b.Service)
                .Where(b => !b.IsDeleted);
            // Apply filters if provided
            if (customerId != null)
            {
                query = query.Where(b => b.Customer!.Id == customerId);
            }
            if (startDate.HasValue)
            {
                query = query.Where(b => b.StartTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(b => b.EndTime <= endDate.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }
            // Order the results
            query = query.OrderByDescending(b => b.BookingDate);
            // Paginate the results
            var bookings = await _unitOfWork.GetRepository<Booking>().GetPagging(query, pageIndex, pageSize);
            return _mapper.Map<PaginatedList<BookingDTO>>(bookings);
        }
    }
}
