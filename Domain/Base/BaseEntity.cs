using Core.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Base
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedTime = LastUpdatedTime = CoreHelper.SystemTimeNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }
        [Display(Name = "Last Updated By")]
        public string? LastUpdatedBy { get; set; }
        [Display(Name = "Deleted By")]
        public string? DeletedBy { get; set; }
        [Display(Name = "Created Time")]
        public DateTimeOffset CreatedTime { get; set; }
        [Display(Name = "Last Updated Time")]
        public DateTimeOffset LastUpdatedTime { get; set; }
        [Display(Name = "Deleted Time")]
        public DateTimeOffset? DeletedTime { get; set; }
    }
}
