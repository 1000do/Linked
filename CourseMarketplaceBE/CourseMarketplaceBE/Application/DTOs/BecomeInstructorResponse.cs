namespace CourseMarketplaceBE.Application.DTOs;

/// <summary>
/// Response trả về sau khi tạo Stripe account (Bước 1 — chưa lưu DB).
/// Chứa URL Onboarding + StripeAccountId để Frontend dùng cho bước hoàn tất.
/// </summary>
public class BecomeInstructorResponse
{
    /// <summary>URL Stripe Onboarding để redirect user</summary>
    public string OnboardingUrl { get; set; } = null!;

    /// <summary>Stripe Account ID (acct_xxx) — dùng để verify sau khi hoàn tất</summary>
    public string StripeAccountId { get; set; } = null!;
}
