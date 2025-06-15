using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAppOverviewService
    {
        Task<AppOverviewDTO> GetOverviewSummaryAsync();
    }
}
