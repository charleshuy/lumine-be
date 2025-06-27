namespace Application.DTOs
{
    public class ProvinceDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class DistrictDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class LocationDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public List<DistrictDto>? Districts { get; set; }
    }
}
