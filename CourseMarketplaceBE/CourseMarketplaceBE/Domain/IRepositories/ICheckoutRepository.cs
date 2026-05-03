using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

/// <summary>
/// SOLID — ISP (Interface Segregation):
/// Repository riêng cho luồng Checkout, KHÔNG gộp vào ICartRepository.
/// Mỗi repository chỉ phục vụ 1 use case rõ ràng.
/// </summary>
public interface ICheckoutRepository
{
    // ── Đọc dữ liệu ─────────────────────────────────────────────────────────
    /// <summary>Lấy cart items kèm Course → Instructor navigation để tính payout.</summary>
    Task<List<CartItem>> GetCartItemsWithCourseAndInstructorAsync(int userId);

    /// <summary>Lấy coupon hợp lệ theo code (kiểm tra active, thời hạn, usage).</summary>
    Task<Coupon?> GetValidCouponAsync(string couponCode, DateTime now);

    /// <summary>Kiểm tra user đã enrolled course chưa (tránh mua trùng).</summary>
    Task<bool> IsEnrolledAsync(int userId, int courseId);
    Task<Enrollment?> GetEnrollmentAsync(int userId, int courseId);
    Task<Enrollment?> GetEnrollmentWithProgressAsync(int userId, int courseId);

    /// <summary>Lấy email của user từ account (gửi cho Stripe).</summary>
    Task<string?> GetUserEmailAsync(int userId);

    // ── Tìm transaction theo Stripe Session ID (cho success callback) ─────────
    /// <summary>Tìm tất cả transactions liên kết với 1 Stripe session.</summary>
    Task<List<Transaction>> GetTransactionsBySessionIdAsync(string stripeSessionId);

    /// <summary>Lấy OrderInfo theo orderId (kèm OrderItems → Course → Instructor).</summary>
    Task<OrderInfo?> GetOrderWithDetailsAsync(int orderId);

    /// <summary>Lấy Stripe Connected Account ID của instructor (cần để tạo Transfer).</summary>
    Task<string?> GetInstructorStripeAccountIdAsync(int instructorId);

    /// <summary>Lấy Stripe Country của instructor để tính toán tiền tệ.</summary>
    Task<string?> GetInstructorStripeCountryAsync(int instructorId);

    // ── Ghi dữ liệu ─────────────────────────────────────────────────────────
    Task AddOrderAsync(OrderInfo order);
    Task AddOrderItemAsync(OrderItem item);
    Task AddTransactionAsync(Transaction transaction);
    Task AddEnrollmentAsync(Enrollment enrollment);
    Task AddEnrollmentProgressAsync(EnrollmentProgress progress);
    Task AddInstructorPayoutAsync(InstructorPayout payout);

    /// <summary>Xóa toàn bộ cart items của user sau khi thanh toán thành công.</summary>
    Task ClearCartAsync(int userId);

    /// <summary>Tăng used_count của coupon.</summary>
    Task IncrementCouponUsageAsync(int couponId);

    // ── Unit of Work ─────────────────────────────────────────────────────────
    /// <summary>Bắt đầu DB transaction (EF Core IDbContextTransaction).</summary>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
}
