using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services
{
    public class CouponService : ICouponService
    {
        private readonly ICouponRepository _repo;

        public CouponService(ICouponRepository repo)
        {
            _repo = repo;
        }

        // ===== HELPER =====
        private string NormalizeType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new Exception("CouponType is required");

            type = type.Trim().ToLower();

            if (type == "fixed" || type == "amount") //coupon type = fixed trừ thẳng giá
                return "fixed";

            if (type == "percent" || type == "percentage")  //coupon type = percent trừ phần trăm
                return "percentage";

            throw new Exception("Invalid CouponType (fixed | percentage)");
        }

        private void ValidateCoupon(string type, decimal value, DateTime? start, DateTime? end, int? usageLimit)
        {
            if (value <= 0)
                throw new Exception("DiscountValue must be > 0");

            if (type == "percentage" && value > 100)
                throw new Exception("Percentage cannot exceed 100");

            if (start.HasValue && end.HasValue && start > end)
                throw new Exception("StartDate must be before EndDate");

            if (usageLimit.HasValue && usageLimit < 0)
                throw new Exception("UsageLimit cannot be negative");
        }

        // ===== CRUD =====

        public async Task<List<CouponResponse>> GetAll(int managerId, bool? isActive, string? type, string? search)
        {
            var data = await _repo.GetAllAsync(managerId, isActive, type, search);

            return data.Select(x => new CouponResponse
            {
                CouponId      = x.CouponId,
                CouponCode    = x.CouponCode,
                CouponType    = x.CouponType ?? "fixed",
                DiscountValue = x.DiscountValue,
                MinOrderValue = x.MinOrderValue,
                StartDate     = x.StartDate,
                EndDate       = x.EndDate,
                UsageLimit    = x.UsageLimit,
                UsedCount     = x.UsedCount,
                IsActive      = x.IsActive
            }).ToList();
        }

        public async Task<CouponResponse?> GetById(int id, int managerId)
        {
            var x = await _repo.GetByIdAsync(id, managerId);
            if (x == null) return null;

            return new CouponResponse
            {
                CouponId      = x.CouponId,
                CouponCode    = x.CouponCode,
                CouponType    = x.CouponType ?? "fixed",
                DiscountValue = x.DiscountValue,
                MinOrderValue = x.MinOrderValue,
                StartDate     = x.StartDate,
                EndDate       = x.EndDate,
                UsageLimit    = x.UsageLimit,
                UsedCount     = x.UsedCount,
                IsActive      = x.IsActive
            };
        }

        public async Task Create(CreateCouponRequest req, int managerId)
        {
            var type = NormalizeType(req.CouponType);

            ValidateCoupon(type, req.DiscountValue, req.StartDate, req.EndDate, req.UsageLimit);

            var coupon = new Coupon
            {
                CouponCode    = req.CouponCode.Trim(),
                CouponType    = type,
                DiscountValue = req.DiscountValue,
                MinOrderValue = req.MinOrderValue < 0 ? 0 : req.MinOrderValue,
                StartDate     = req.StartDate,
                EndDate       = req.EndDate,
                UsageLimit    = req.UsageLimit,
                UsedCount     = 0,
                IsActive      = req.IsActive,
                ManagerId     = managerId
            };

            await _repo.AddAsync(coupon);
            await _repo.SaveChangesAsync();
        }

        public async Task Update(int id, UpdateCouponRequest req, int managerId)
        {
            var coupon = await _repo.GetByIdAsync(id, managerId);
            if (coupon == null) throw new Exception("Coupon not found");

            // giữ type cũ nếu không update
            var newType  = req.CouponType != null ? NormalizeType(req.CouponType) : coupon.CouponType!;
            var newValue = req.DiscountValue ?? coupon.DiscountValue;
            var newStart = req.StartDate ?? coupon.StartDate;
            var newEnd   = req.EndDate ?? coupon.EndDate;
            var newUsage = req.UsageLimit ?? coupon.UsageLimit;

            ValidateCoupon(newType, newValue, newStart, newEnd, newUsage);

            if (req.CouponCode != null)
                coupon.CouponCode = req.CouponCode.Trim();

            coupon.CouponType    = newType;
            coupon.DiscountValue = newValue;
            coupon.MinOrderValue = req.MinOrderValue.HasValue
                ? Math.Max(0, req.MinOrderValue.Value)
                : coupon.MinOrderValue;
            coupon.StartDate  = newStart;
            coupon.EndDate    = newEnd;
            coupon.UsageLimit = newUsage;

            if (req.IsActive.HasValue)
                coupon.IsActive = req.IsActive;

            _repo.Update(coupon);
            await _repo.SaveChangesAsync();
        }

        public async Task Delete(int id, int managerId)
        {
            var coupon = await _repo.GetByIdAsync(id, managerId);
            if (coupon == null) throw new Exception("Coupon not found");

            _repo.Delete(coupon);
            await _repo.SaveChangesAsync();
        }

        // ===== APPLY LOGIC (QUAN TRỌNG) =====
        public decimal ApplyCoupon(Coupon coupon, decimal originalPrice)
        {
            if (coupon.IsActive != true)
                throw new Exception("Coupon is not active");

            if (coupon.StartDate.HasValue && DateTime.UtcNow < coupon.StartDate)
                throw new Exception("Coupon not started");

            if (coupon.EndDate.HasValue && DateTime.UtcNow > coupon.EndDate)
                throw new Exception("Coupon expired");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
                throw new Exception("Coupon usage limit reached");

            // Kiểm tra giá trị đơn tối thiểu
            if (coupon.MinOrderValue > 0 && originalPrice < coupon.MinOrderValue)
                throw new Exception($"Giá trị đơn hàng tối thiểu để dùng mã này là {coupon.MinOrderValue:N0} VND.");

            decimal finalPrice;

            if (coupon.CouponType == "percentage")
            {
                finalPrice = originalPrice - (originalPrice * coupon.DiscountValue / 100);
            }
            else
            {
                finalPrice = originalPrice - coupon.DiscountValue;
            }

            // chống âm
            return Math.Max(finalPrice, 0);
        }
    }
}
