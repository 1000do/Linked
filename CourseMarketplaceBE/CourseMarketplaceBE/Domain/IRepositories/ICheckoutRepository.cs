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

    // ── Tìm transaction theo Stripe Session ID (cho success callback) ─────────
    /// <summary>Tìm tất cả transactions liên kết with 1 Stripe session.</summary>
    Task<List<Transaction>> GetTransactionsBySessionIdAsync(string stripeSessionId);

    Task<List<Transaction>> GetTransactionsByOrderIdAsync(int orderId);

    /// <summary>Lấy OrderInfo theo orderId (kèm OrderItems → Course → Instructor).</summary>
    Task<OrderInfo?> GetOrderWithDetailsAsync(int orderId);

    /// <summary>Lấy danh sách đơn hàng pending của user để dọn dẹp trước khi tạo checkout mới.</summary>
    Task<List<OrderInfo>> GetPendingOrdersByUserAsync(int userId);

    // ── Ghi dữ liệu ─────────────────────────────────────────────────────────
    Task AddOrderAsync(OrderInfo order);
    Task AddOrderItemAsync(OrderItem item);
    Task AddTransactionAsync(Transaction transaction);
    Task AddInstructorPayoutAsync(InstructorPayout payout);

    /// <summary>Xóa toàn bộ cart items của user sau khi thanh toán thành công.</summary>
    Task ClearCartAsync(int userId);

    /// <summary>Tăng used_count của coupon.</summary>
    Task IncrementCouponUsageAsync(int couponId);

    Task DeleteOrderAsync(OrderInfo order);
    Task DeleteTransactionsAsync(IEnumerable<Transaction> transactions);

    // ── Unit of Work ─────────────────────────────────────────────────────────
    /// <summary>Bắt đầu DB transaction (EF Core IDbContextTransaction).</summary>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
}
