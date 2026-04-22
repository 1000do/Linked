using System;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class InstructorPayout
{
    public int PayoutId { get; set; }

    public int? TransactionId { get; set; }

    public int? InstructorId { get; set; }

    public decimal PayoutAmount { get; set; }

    public DateTime PayoutDate { get; set; }

    public bool IsPaid { get; set; } = false;

    public virtual Transaction? Transaction { get; set; }

    public virtual Instructor? Instructor { get; set; }
}
