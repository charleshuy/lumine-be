using Domain.Base;

namespace Domain.Entities
{
    public class District : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public Guid ProvinceId { get; set; }
        public Province Province { get; set; } = null!;

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }

}
