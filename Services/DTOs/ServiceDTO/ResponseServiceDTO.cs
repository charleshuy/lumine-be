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
        public Guid? ArtistID { get; set; }
        public ServiceTypeDTO? ServiceType { get; set; }
    }
    public class CreateServiceDTO
    {
        public string ServiceName { get; set; } = default!;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }
        public Guid ArtistID { get; set; }
        public Guid ServiceTypeID { get; set; }
    }

    public class CreateServiceForArtistDTO
    {
        public string ServiceName { get; set; } = default!;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }
        public Guid ServiceTypeID { get; set; }
    }

    public class UpdateServiceDTO
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = default!;
        public string? Description { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }
        public Guid ServiceTypeID { get; set; }
    }

    public class BookingServiceDTO
    {
        public Guid Id { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public string? Status { get; set; }
        public string? ArtistName { get; set; }
        public Guid? ArtistID { get; set; }
    }
}
