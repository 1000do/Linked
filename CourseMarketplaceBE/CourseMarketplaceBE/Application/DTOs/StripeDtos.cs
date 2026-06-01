using System;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class StripeConnectSetupResponse
    {
        public string StripeAccountId { get; set; } = null!;
        public string OnboardingUrl { get; set; } = null!;
    }

    public class StripeAccountStatusDto
    {
        public bool DetailsSubmitted { get; set; }
        public bool PayoutsEnabled { get; set; }
        public bool ChargesEnabled { get; set; }
    }

    public class StripePayoutDto
    {
        public string Id { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? ArrivalDate { get; set; }
    }

    public class StripeBalanceTransactionDto
    {
        public string Type { get; set; } = null!;
        public string? SourceId { get; set; }
    }

    public class StripePlatformBalanceDto
    {
        public decimal Available { get; set; }
        public decimal Incoming { get; set; }
        public string Currency { get; set; } = "usd";
        public string PayoutScheduleInterval { get; set; } = "manual";
        public string? PayoutScheduleAnchor { get; set; }
    }

    public class StripeTransferResponseDto
    {
        public string Id { get; set; } = null!;
        public string DestinationPaymentId { get; set; } = null!;
        public decimal DestinationAmount { get; set; }
    }

    public class StripeWithdrawalResponseDto
    {
        public string Id { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? ArrivalDate { get; set; }
    }
}
