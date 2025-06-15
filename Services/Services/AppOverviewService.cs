using Application.DTOs;
using Application.Interfaces.Services;
using Application.Interfaces.UOW;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AppOverviewService : IAppOverviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppOverviewService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<AppOverviewDTO> GetOverviewSummaryAsync()
        {
            var userRepo = _unitOfWork.GetRepository<ApplicationUser>();
            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            var allUsers = await userRepo.Entities
                .Where(u => !u.IsDeleted)
                .ToListAsync();

            var customers = await _userManager.GetUsersInRoleAsync("User");
            var artists = await _userManager.GetUsersInRoleAsync("Artist");

            int totalUsers = customers.Count(u => !u.IsDeleted);
            int totalPartners = artists.Count(u => !u.IsDeleted);

            int totalCompletedBookings = await bookingRepo.Entities
                .CountAsync(b => !b.IsDeleted && b.Status == BookingStatus.Completed);

            decimal totalRevenue = await bookingRepo.Entities
                .Where(b => !b.IsDeleted && b.Status == BookingStatus.Completed)
                .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

            return new AppOverviewDTO
            {
                TotalUsers = totalUsers,
                TotalPartners = totalPartners,
                TotalCompletedBookings = totalCompletedBookings,
                TotalRevenue = totalRevenue
            };
        }
    }
}
