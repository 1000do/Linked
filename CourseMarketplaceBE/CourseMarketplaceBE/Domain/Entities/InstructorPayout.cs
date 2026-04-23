using System;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>
/// Lưu dữ liệu giao dịch chuyển tiền từ hệ thống sang tài khoản ngân hàng của instructor.
/// </summary>
public partial class InstructorPayout
{
    public int PayoutId { get; set; }

    public int? TransactionId { get; set; }

    public int? InstructorId { get; set; }

    /// <summary>Số tiền sẽ chuyển cho instructor (đã trừ phần sàn ăn)</summary>
    public decimal PayoutAmount { get; set; }

    /// <summary>Ngày hệ thống sẽ chuyển tiền (theo lịch)</summary>
    public DateTime PayoutDate { get; set; }

    public bool IsPaid { get; set; }

    public virtual Transaction? Transaction { get; set; }

    public virtual Instructor? Instructor { get; set; }
}
