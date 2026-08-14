using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Domain.Entities
{
    public class MaterialExt
    {
        [Key]
        public int MaterialId { get; set; }
        public string? FileHash { get; set; }

        public virtual LearningMaterial LearningMaterial { get; set; } = null!;
    }
}
