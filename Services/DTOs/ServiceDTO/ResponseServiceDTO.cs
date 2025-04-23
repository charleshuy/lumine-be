using Domain.Entities;

namespace Application.DTOs.ServiceDTO
{
    public class ResponseServiceDTO
    {
        public Guid Id { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }
        public string? ArtistName { get; set; }
    }
}
