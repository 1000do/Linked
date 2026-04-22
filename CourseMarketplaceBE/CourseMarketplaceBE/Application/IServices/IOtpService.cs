namespace CourseMarketplaceBE.Application.IServices
{
    public interface IOtpService
    {
        string GenerateOtp(string email);
        bool VerifyOtp(string email, string otp);
    }
}
