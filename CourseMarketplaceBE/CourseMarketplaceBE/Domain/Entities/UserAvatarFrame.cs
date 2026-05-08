using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseMarketplaceBE.Domain.Entities
{
    [Table("user_avatar_frames")]
    public class UserAvatarFrame
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("frame_id")]
        public int FrameId { get; set; }

        [Column("unlocked_at")]
        public DateTime UnlockedAt { get; set; } = DateTime.Now;

        [Column("is_equipped")]
        public bool IsEquipped { get; set; } = false;

        [ForeignKey("FrameId")]
        public virtual AvatarFrame Frame { get; set; } = null!;
    }
}
