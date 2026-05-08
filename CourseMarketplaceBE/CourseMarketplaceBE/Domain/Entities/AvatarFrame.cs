using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseMarketplaceBE.Domain.Entities
{
    [Table("avatar_frames")]
    public class AvatarFrame
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("image_url")]
        public string ImageUrl { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("requirement_type")]
        public string? RequirementType { get; set; }

        [Column("requirement_value")]
        public int RequirementValue { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
