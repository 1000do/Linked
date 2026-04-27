using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Application.Services;

/// <summary>
/// Service xử lý Giỏ hàng và Mã giảm giá.
/// - userId luôn được lấy từ JWT Claim bởi Controller, tuyệt đối KHÔNG nhận từ body/query.
/// </summary>
public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 1. THÊM VÀO GIỎ HÀNG (UC-21)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task AddToCartAsync(int userId, int courseId)
    {
        // Kiểm tra khóa học có tồn tại và đang được published không
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == courseId && c.CourseStatus == "published");

        if (course == null)
            throw new InvalidOperationException("Khóa học không tồn tại hoặc chưa được phát hành.");

        // Kiểm tra user đã enrolled chưa (không cần mua lại)
        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (alreadyEnrolled)
            throw new InvalidOperationException("Bạn đã mua khóa học này rồi.");

        // Kiểm tra khóa học đã có trong giỏ chưa
        var alreadyInCart = await _context.CartItems
            .AnyAsync(c => c.UserId == userId && c.CourseId == courseId);
        if (alreadyInCart)
            throw new InvalidOperationException("Khóa học đã có trong giỏ hàng.");

        // Thêm vào giỏ với giá hiện tại của khóa học (snapshot price)
        var cartItem = new CartItem
        {
            UserId = userId,
            CourseId = courseId,
            Price = course.Price,           // Lưu snapshot giá tại thời điểm add
            AddedDate = DateTime.Now
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. XÓA KHỎI GIỎ HÀNG (UC-22)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task RemoveFromCartAsync(int userId, int courseId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

        if (item == null)
            throw new InvalidOperationException("Không tìm thấy khóa học trong giỏ hàng.");

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. LẤY TÓM TẮT GIỎ HÀNG + TÍNH TIỀN COUPON (UC-20, UC-23)
    // ═══════════════════════════════════════════════════════════════════════
    public async Task<CartSummaryResponse> GetCartSummaryAsync(int userId, string? couponCode)
    {
        // ── 3.1 Lấy toàn bộ cart_items của user kèm thông tin khóa học + instructor ──
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .Include(c => c.Course)
                .ThenInclude(course => course!.Instructor)
                    .ThenInclude(i => i!.InstructorNavigation)   // User.FullName
            .OrderBy(c => c.AddedDate)
            .ToListAsync();

        // ── 3.2 Map sang DTO ──────────────────────────────────────────────
        var items = cartItems.Select(c => new CartItemDto
        {
            CourseId   = c.CourseId ?? 0,
            Title      = c.Course?.Title ?? "(Khóa học không tìm thấy)",
            ThumbnailUrl = c.Course?.CourseThumbnailUrl,
            // Lấy FullName giảng viên qua chuỗi navigation Instructor → User
            InstructorName = c.Course?.Instructor?.InstructorNavigation?.FullName,
            // Ưu tiên giá snapshot lúc add; nếu null thì lấy giá hiện tại của course
            Price = c.Price ?? c.Course?.Price ?? 0m
        }).ToList();

        // ── 3.3 Tính SubTotal (tổng tiền trước giảm giá) ─────────────────
        // SubTotal = tổng tất cả items.Price
        decimal subTotal = items.Sum(i => i.Price);

        // ── 3.4 Khởi tạo response với giá trị mặc định (không coupon) ────
        var response = new CartSummaryResponse
        {
            Items          = items,
            SubTotal       = subTotal,
            DiscountAmount = 0m,
            Total          = subTotal,          // Total = SubTotal khi không có coupon
            AppliedCouponCode = null,
            CouponMessage  = null
        };

        // ── 3.5 Kiểm tra và tính Coupon nếu có ───────────────────────────
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var now = DateTime.Now;

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(cp =>
                    cp.CouponCode.ToLower() == couponCode.Trim().ToLower());

            if (coupon == null)
            {
                // Mã không tồn tại trong hệ thống
                response.CouponMessage = "Mã giảm giá không tồn tại.";
            }
            else if (coupon.IsActive != true)
            {
                // Mã đã bị vô hiệu hóa bởi Admin
                response.CouponMessage = "Mã giảm giá đã bị vô hiệu hóa.";
            }
            else if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
            {
                // Chưa đến thời điểm áp dụng
                response.CouponMessage = $"Mã giảm giá chưa có hiệu lực đến ngày {coupon.StartDate.Value:dd/MM/yyyy}.";
            }
            else if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
            {
                // Coupon đã hết hạn
                response.CouponMessage = $"Mã giảm giá đã hết hạn từ ngày {coupon.EndDate.Value:dd/MM/yyyy}.";
            }
            else if (coupon.UsageLimit.HasValue &&
                     (coupon.UsedCount ?? 0) >= coupon.UsageLimit.Value)
            {
                // Coupon đã đạt giới hạn sử dụng
                response.CouponMessage = "Mã giảm giá đã đạt giới hạn sử dụng.";
            }
            else if (subTotal == 0)
            {
                // Giỏ hàng rỗng, không cần tính giảm giá
                response.CouponMessage = "Giỏ hàng của bạn đang trống.";
            }
            else
            {
                // ✅ Coupon hợp lệ — tính DiscountAmount theo loại
                decimal discountAmount;

                if (coupon.CouponType == "percentage")
                {
                    // Giảm theo % tổng hóa đơn
                    // DiscountAmount = SubTotal * (DiscountValue / 100)
                    discountAmount = subTotal * (coupon.DiscountValue / 100m);
                }
                else // "fixed_amount"
                {
                    // Giảm cố định, không được vượt quá SubTotal
                    discountAmount = coupon.DiscountValue;
                }

                // Đảm bảo DiscountAmount không vượt quá SubTotal (không âm hóa Total)
                discountAmount = Math.Min(discountAmount, subTotal);

                // Làm tròn 2 chữ số thập phân để tránh floating-point dư
                discountAmount = Math.Round(discountAmount, 2);

                // Ghi nhận kết quả vào response
                response.DiscountAmount   = discountAmount;
                response.Total            = Math.Round(subTotal - discountAmount, 2); // Total = SubTotal - Discount
                response.AppliedCouponCode = coupon.CouponCode;
                response.CouponMessage    = coupon.CouponType == "percentage"
                    ? $"Áp dụng thành công! Giảm {coupon.DiscountValue:0.##}% tổng hóa đơn."
                    : $"Áp dụng thành công! Giảm {coupon.DiscountValue:N0} VND.";
            }
        }

        return response;
    }
}
