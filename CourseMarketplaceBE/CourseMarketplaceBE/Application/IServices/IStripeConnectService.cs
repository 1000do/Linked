using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IStripeConnectService
    {
        // ── Instructor Stripe Express Onboarding ──
        Task<StripeConnectSetupResponse> SetupExpressAccountAsync(int userId, string email, string country, string professionalTitle, string expertiseCategories, string? existingStripeAccountId = null);
        Task<StripeAccountStatusDto> GetAccountStatusAsync(string stripeAccountId);
        Task DeleteAccountAsync(string stripeAccountId);
        Task<string> GetLoginLinkAsync(string stripeAccountId);

        // ── Payout & Balance Transaction Synchronization ──
        Task<List<StripePayoutDto>> ListPayoutsAsync(string stripeAccountId);
        Task<List<StripeBalanceTransactionDto>> ListBalanceTransactionsAsync(string stripeAccountId, string stripePayoutId);

        // ── Platform Balance & Withdrawals ──
        Task<StripePlatformBalanceDto> GetPlatformBalanceAsync();
        Task<StripeWithdrawalResponseDto> CreatePlatformWithdrawalAsync(decimal amount, string description, int managerId);
        Task<StripeWithdrawalResponseDto> GetPlatformPayoutStatusAsync(string stripePayoutId);

        // ── Platform Connect Transfers ──
        Task<StripeTransferResponseDto> CreateConnectTransferAsync(decimal amount, string currency, string destinationAccountId, string description, int payoutId);
    }
}
