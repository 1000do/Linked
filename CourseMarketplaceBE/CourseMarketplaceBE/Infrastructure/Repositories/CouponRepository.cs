using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coupon>> GetAllAsync(int managerId, bool? isActive, string? type, string? search)
        {
            var query = _context.Coupons
                .Where(x => x.ManagerId == managerId)
                .AsQueryable();

            // FILTER ACTIVE
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive);
            }

            // FILTER TYPE
            if (!string.IsNullOrWhiteSpace(type))
            {
                type = type.Trim().ToLower();
                query = query.Where(x => x.CouponType == type);
            }

            // SEARCH (KEY PART)
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

        public async Task<Coupon?> GetByIdAsync(int id, int managerId)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(x => x.CouponId == id && x.ManagerId == managerId);
        }

        public async Task AddAsync(Coupon coupon)
        {
            await _context.Coupons.AddAsync(coupon);
        }

        public void Update(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
        }

        public void Delete(Coupon coupon)
        {
            _context.Coupons.Remove(coupon);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
