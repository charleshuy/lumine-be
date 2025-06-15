namespace Application.DTOs
{
    public class AppOverviewDTO
    {
        public int TotalUsers { get; set; }
        public int TotalPartners { get; set; }
        public int TotalCompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
