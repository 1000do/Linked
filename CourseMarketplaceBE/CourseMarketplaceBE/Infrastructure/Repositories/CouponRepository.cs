using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

/// <summary>
/// SOLID — SRP: Chỉ truy vấn dữ liệu coupon + course (cho gắn coupon).
/// SOLID — DIP: Implement ICouponRepository — Service không biết EF Core.
///
/// ★ Performance:
///   - GetActivePlatformCouponsAsync: WHERE is_active AND end_date > NOW AND used_count < usage_limit
///   - Dùng .AsNoTracking() cho query read-only
/// </summary>
public class CouponRepository : ICouponRepository
{
    private readonly AppDbContext _context;

    public CouponRepository(AppDbContext context) => _context = context;

    // ═══════════════════════════════════════════════════════════════════════
    // ADMIN CRUD
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<Coupon>> GetAllAsync(int? managerId, bool? isActive, string? type, string? search)
    {
        var query = _context.Coupons.AsQueryable();

        if (managerId.HasValue)
            query = query.Where(x => x.ManagerId == managerId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(type))
        {
            type = type.Trim().ToLower();
            query = query.Where(x => x.CouponType == type);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(x =>
                (x.CouponCode ?? "").ToLower().Contains(search) ||
                x.CouponId.ToString().Contains(search)
            );
        }

        return await query
            .OrderByDescending(x => x.CouponId)
            .ToListAsync();
    }

    public async Task<Coupon?> GetByIdAsync(int id, int? managerId)
    {
        var query = _context.Coupons.AsQueryable();
        
        if (managerId.HasValue)
            query = query.Where(x => x.ManagerId == managerId.Value);

        return await query.FirstOrDefaultAsync(x => x.CouponId == id);
    }

    public async Task<Coupon?> GetByCodeAsync(string couponCode)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(x => x.CouponCode == couponCode);
    }

    public async Task AddAsync(Coupon coupon)
    {
        await _context.Coupons.AddAsync(coupon);
    }

    public void Update(Coupon coupon)
    {
        _context.Coupons.Update(coupon);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // INSTRUCTOR: Browse & Attach
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<List<Coupon>> GetActivePlatformCouponsAsync()
    {
        var now = DateTime.Now;

        return await _context.Coupons
            .Where(c => c.IsActive == true)
            .Where(c => !c.EndDate.HasValue || c.EndDate > now)      // chưa hết hạn
            .Where(c => !c.StartDate.HasValue || c.StartDate <= now) // đã bắt đầu
            .Where(c => !c.UsageLimit.HasValue || (c.UsedCount ?? 0) < c.UsageLimit) // còn slot
            .OrderByDescending(c => c.CouponId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Coupon?> GetByIdGlobalAsync(int couponId)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.CouponId == couponId);
    }

    public async Task<Course?> GetCourseWithInstructorAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Coupon)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }

    public void UpdateCourse(Course course)
    {
        _context.Courses.Update(course);
    }
}
