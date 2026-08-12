using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface ICheckoutSessionRepository
{
    Task<int> CreateSessionAsync(CheckoutSession session);
    Task<CheckoutSession?> GetSessionByIdAsync(string sessionId);
    Task<int> UpdateSessionAsync(CheckoutSession session);
}
