using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using Stripe;
using Microsoft.Extensions.Configuration;

namespace CourseMarketplaceBE.Infrastructure.Services
{
    public class StripeConnectService : IStripeConnectService
    {
        private readonly IConfiguration _configuration;

        public StripeConnectService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<StripeConnectSetupResponse> SetupExpressAccountAsync(int userId, string email, string country, string professionalTitle, string expertiseCategories, string? existingStripeAccountId = null)
        {
            var frontendUrl = _configuration.GetValue<string>("FrontendBaseUrl") ?? "http://localhost:5208";
            var onboardingType = "account_onboarding";
            string stripeAccountId = existingStripeAccountId ?? "";

            if (!string.IsNullOrEmpty(stripeAccountId))
            {
                var reLinkService = new AccountLinkService();
                var reLink = await reLinkService.CreateAsync(new AccountLinkCreateOptions
                {
                    Account = stripeAccountId,
                    RefreshUrl = $"{frontendUrl}/Instructor/OnboardingRefresh",
                    ReturnUrl = $"{frontendUrl}/Instructor/StripeReturn?instructorId={userId}",
                    Type = onboardingType
                });

                return new StripeConnectSetupResponse
                {
                    OnboardingUrl = reLink.Url,
                    StripeAccountId = stripeAccountId
                };
            }

            var accountService = new AccountService();
            var stripeAccount = await accountService.CreateAsync(new AccountCreateOptions
            {
                Type = "express",
                Country = !string.IsNullOrEmpty(country) ? country : "SG",
                Email = email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                },
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    ProductDescription = $"{professionalTitle} | {expertiseCategories}"
                },
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId.ToString() },
                    { "instructor_id", userId.ToString() }
                }
            });

            var linkService = new AccountLinkService();
            var link = await linkService.CreateAsync(new AccountLinkCreateOptions
            {
                Account = stripeAccount.Id,
                RefreshUrl = $"{frontendUrl}/Instructor/OnboardingRefresh",
                ReturnUrl = $"{frontendUrl}/Instructor/StripeReturn?instructorId={userId}",
                Type = onboardingType
            });

            return new StripeConnectSetupResponse
            {
                OnboardingUrl = link.Url,
                StripeAccountId = stripeAccount.Id
            };
        }

        public async Task<StripeAccountStatusDto> GetAccountStatusAsync(string stripeAccountId)
        {
            var accountService = new AccountService();
            var stripeAccount = await accountService.GetAsync(stripeAccountId);

            return new StripeAccountStatusDto
            {
                DetailsSubmitted = stripeAccount.DetailsSubmitted,
                PayoutsEnabled = stripeAccount.PayoutsEnabled,
                ChargesEnabled = stripeAccount.ChargesEnabled
            };
        }

        public async Task DeleteAccountAsync(string stripeAccountId)
        {
            var accountService = new AccountService();
            await accountService.DeleteAsync(stripeAccountId);
        }

        public async Task<string> GetLoginLinkAsync(string stripeAccountId)
        {
            var loginLinkService = new AccountLoginLinkService();
            var loginLink = await loginLinkService.CreateAsync(stripeAccountId);
            return loginLink.Url;
        }

        public async Task<List<StripePayoutDto>> ListPayoutsAsync(string stripeAccountId)
        {
            var payoutService = new PayoutService();
            var stripePayouts = await payoutService.ListAsync(new PayoutListOptions
            {
                Limit = 100,
                Expand = new List<string> { "data.destination" }
            }, new RequestOptions
            {
                StripeAccount = stripeAccountId
            });

            return stripePayouts.Select(p => new StripePayoutDto
            {
                Id = p.Id,
                Status = p.Status,
                ArrivalDate = p.ArrivalDate
            }).ToList();
        }

        public async Task<List<StripeBalanceTransactionDto>> ListBalanceTransactionsAsync(string stripeAccountId, string stripePayoutId)
        {
            var btService = new BalanceTransactionService();
            var balanceTransactions = await btService.ListAsync(new BalanceTransactionListOptions
            {
                Payout = stripePayoutId,
                Limit = 100
            }, new RequestOptions
            {
                StripeAccount = stripeAccountId
            });

            return balanceTransactions.Select(b => new StripeBalanceTransactionDto
            {
                Type = b.Type,
                SourceId = b.SourceId
            }).ToList();
        }

        public async Task<StripePlatformBalanceDto> GetPlatformBalanceAsync()
        {
            var balanceService = new BalanceService();
            var balance = await balanceService.GetAsync();

            var available = balance.Available?.FirstOrDefault(b => b.Currency == "usd");
            var pending = balance.Pending?.FirstOrDefault(b => b.Currency == "usd");

            decimal availableAmount = available != null ? (decimal)available.Amount / 100m : 0m;
            decimal pendingAmount = pending != null ? (decimal)pending.Amount / 100m : 0m;

            var accountService = new AccountService();
            string scheduleInterval = "manual";
            string? scheduleAnchor = null;

            try
            {
                var account = await accountService.GetSelfAsync();
                if (account?.Settings?.Payouts?.Schedule != null)
                {
                    scheduleInterval = account.Settings.Payouts.Schedule.Interval ?? "manual";
                    scheduleAnchor = account.Settings.Payouts.Schedule.WeeklyAnchor 
                        ?? account.Settings.Payouts.Schedule.MonthlyAnchor.ToString();
                }
            }
            catch { /* Ignore if no permission */ }

            return new StripePlatformBalanceDto
            {
                Available = availableAmount,
                Incoming = pendingAmount,
                Currency = "usd",
                PayoutScheduleInterval = scheduleInterval,
                PayoutScheduleAnchor = scheduleAnchor
            };
        }

        public async Task<StripeWithdrawalResponseDto> CreatePlatformWithdrawalAsync(decimal amount, string description, int managerId)
        {
            var payoutService = new PayoutService();
            var stripePayout = await payoutService.CreateAsync(new PayoutCreateOptions
            {
                Amount = (long)(amount * 100), // convert to cents
                Currency = "usd",
                Description = description,
                Metadata = new Dictionary<string, string>
                {
                    { "manager_id", managerId.ToString() },
                    { "source", "web_dashboard" }
                }
            });

            return new StripeWithdrawalResponseDto
            {
                Id = stripePayout.Id,
                Status = stripePayout.Status,
                ArrivalDate = stripePayout.ArrivalDate
            };
        }

        public async Task<StripeWithdrawalResponseDto> GetPlatformPayoutStatusAsync(string stripePayoutId)
        {
            var payoutService = new PayoutService();
            var stripePayout = await payoutService.GetAsync(stripePayoutId);

            return new StripeWithdrawalResponseDto
            {
                Id = stripePayout.Id,
                Status = stripePayout.Status,
                ArrivalDate = stripePayout.ArrivalDate
            };
        }

        public async Task<StripeTransferResponseDto> CreateConnectTransferAsync(decimal amount, string currency, string destinationAccountId, string description, int payoutId)
        {
            var transferService = new TransferService();
            var transfer = await transferService.CreateAsync(new TransferCreateOptions
            {
                Amount = (long)(amount * 100), 
                Currency = currency.ToLower(),
                Destination = destinationAccountId,
                Description = description,
                Metadata = new Dictionary<string, string> { { "payout_id", payoutId.ToString() } }
            });

            decimal destAmount = amount;
            try 
            {
                var requestOptions = new RequestOptions { StripeAccount = destinationAccountId };
                var chargeService = new ChargeService();
                var destinationCharge = await chargeService.GetAsync(transfer.DestinationPaymentId, null, requestOptions);
                if (destinationCharge != null)
                    destAmount = (decimal)destinationCharge.Amount / 100m;
            }
            catch { /* Fallback to source amount if access is restricted */ }

            return new StripeTransferResponseDto
            {
                Id = transfer.Id,
                DestinationPaymentId = transfer.DestinationPaymentId,
                DestinationAmount = destAmount
            };
        }
    }
}
