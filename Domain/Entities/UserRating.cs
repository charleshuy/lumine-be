namespace Domain.Entities
{
    public class UserRating
    {
        public Guid Id { get; set; }
        public Guid ArtistId { get; set; }
        public ApplicationUser Artist { get; set; } = default!;

        public Guid CustomerId { get; set; }
        public ApplicationUser Customer { get; set; } = default!;

        public double Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }

}
