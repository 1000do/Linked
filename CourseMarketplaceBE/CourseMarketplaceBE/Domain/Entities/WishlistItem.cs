using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class WishlistItem
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? CourseId { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual Course? Course { get; set; }

    public virtual User? User { get; set; }
}
