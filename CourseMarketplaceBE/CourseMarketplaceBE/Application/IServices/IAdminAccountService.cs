using CourseMarketplaceBE.Application.DTOs;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IAdminAccountService
    {
        Task<CourseMarketplaceBE.Application.DTOs.Common.PagedResult<AdminAccountListDto>> GetAccountsPagedAsync(string? keyword, string? role, int page, int pageSize);
        Task<AdminAccountDetailDto?> GetAccountDetailAsync(int id);
        Task<AccountTransactionSummaryDto?> GetAccountTransactionsAsync(int id);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsUsernameExistsAsync(string username);
        Task CreateStaffAsync(CreateStaffRequest request);
        Task<bool> UpdateStaffAsync(int id, UpdateStaffRequest request);
        Task<string?> ToggleBanAsync(int id, int adminId);
        Task<(bool Success, int CurrentFlags, string? NewStatus, string? ErrorMessage)> FlagAccountAsync(int id, string reason);
    }
}
