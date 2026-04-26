using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface ICouponService
    {
        Task<List<CouponResponse>> GetAll(int managerId, bool? isActive, string? type, string? search);
        Task<CouponResponse?> GetById(int id, int managerId);
        Task Create(CreateCouponRequest request, int managerId);
        Task Update(int id, UpdateCouponRequest request, int managerId);
        Task Delete(int id, int managerId);
    }
}
