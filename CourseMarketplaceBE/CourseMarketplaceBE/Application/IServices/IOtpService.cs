namespace CourseMarketplaceBE.Application.IServices
{
    public interface IOtpService
    {
        string GenerateOtp(string email, string purpose);
        bool ValidateOtp(string email, string otp, string purpose); // ❗ chỉ check

        bool ConsumeOtp(string email, string otp, string purpose);  // ❗ check + xoá
    }
}
