using Domain.Entities;

namespace Application.DTOs
{
    public class RatingDTO
    {
        public Guid Id { get; set; }
        public Guid ArtistId { get; set; }
        public string Review { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public double Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }

    public class RatingRequestDTO
    {
        public Guid ArtistId { get; set; }
        public string Review { get; set; } = string.Empty;
        public double Rating { get; set; }
    }
}
