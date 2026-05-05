using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

/// <summary>
/// Interface xử lý nghiệp vụ Giỏ hàng:
/// 1. Thêm khóa học vào giỏ (UC-21)
/// 2. Xóa khóa học khỏi giỏ (UC-22)
/// 3. Lấy tóm tắt giỏ hàng + tính tiền coupon (UC-20, UC-23)
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Thêm khóa học vào giỏ của userId.
    /// Ném InvalidOperationException nếu đã tồn tại hoặc khóa học không hợp lệ.
    /// </summary>
    Task AddToCartAsync(int userId, int courseId);

    /// <summary>
    /// Xóa khóa học khỏi giỏ của userId.
    /// Ném InvalidOperationException nếu item không tồn tại.
    /// </summary>
    Task RemoveFromCartAsync(int userId, int courseId);

    /// <summary>
    /// Lấy toàn bộ giỏ hàng của userId, tính toán coupon nếu có.
    /// </summary>
    /// <param name="userId">ID người dùng đang login (từ JWT).</param>
    /// <param name="couponCode">Mã giảm giá (null = không áp dụng).</param>
    Task<CartSummaryResponse> GetCartSummaryAsync(int userId, string? couponCode);
}
