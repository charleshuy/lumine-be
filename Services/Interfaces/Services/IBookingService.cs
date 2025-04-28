using Application.DTOs;
using Application.DTOs.ServiceDTO;
using Application.Paggings;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<PaginatedList<BookingDTO>> GetBookingsAsync(
            int pageIndex,
            int pageSize,
            Guid? customerId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            BookingStatus? status = null);
    }
}
