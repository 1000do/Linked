using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories
{
    public interface ICouponRepository
    {
        Task<List<Coupon>> GetAllAsync(int managerId, bool? isActive, string? type, string? search);
        Task<Coupon?> GetByIdAsync(int id, int managerId);
        Task AddAsync(Coupon coupon);
        void Update(Coupon coupon);
        void Delete(Coupon coupon);
        Task SaveChangesAsync();
    }
}
