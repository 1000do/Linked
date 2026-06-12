using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IGiftRepository
{
    Task<Gift?> GetByIdAsync(int giftId);
    
    Task<Gift?> GetByTokenAsync(string token);
    
    Task<Gift?> GetByOrderItemIdAsync(int orderItemId);
    
    Task AddAsync(Gift gift);
    
    void Update(Gift gift);
    
    Task<int> SaveChangesAsync();
}
