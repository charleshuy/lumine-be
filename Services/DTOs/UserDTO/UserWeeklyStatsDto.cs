namespace Application.DTOs.UserDTO
{
    public class UserWeeklyStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int WeekNumber { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
