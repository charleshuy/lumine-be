using Domain.Base;

namespace Domain.Entities
{
    public class Province : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<District> Districts { get; set; } = new List<District>();
    }

}
