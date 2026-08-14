using CourseMarketplaceBE.Domain.Entities;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IGiftCheckoutSessionRepository
{
    Task<GiftCheckoutSession?> GetByIdAsync(string id);
    Task AddAsync(GiftCheckoutSession session);
    Task<int> SaveChangesAsync();
}
