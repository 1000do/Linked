using CourseMarketplaceBE.Application.IServices;
using System.Collections.Concurrent;


namespace CourseMarketplaceBE.Application.Services
{
    public class OtpService : IOtpService
    {
        private class OtpInfo
        {
            public string Code { get; set; } = null!;
            public DateTime ExpireAt { get; set; }
        }

        private static readonly ConcurrentDictionary<string, OtpInfo> _store = new();

        public string GenerateOtp(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            _store[email] = new OtpInfo
            {
                Code = otp,
                ExpireAt = DateTime.UtcNow.AddMinutes(2) 
            };

            return otp;
        }

        public bool VerifyOtp(string email, string otp)
        {
            if (!_store.TryGetValue(email, out var info))
                return false;

            if (DateTime.UtcNow > info.ExpireAt)
            {
                _store.TryRemove(email, out _);
                return false;
            }

            if (info.Code != otp)
                return false;

            _store.TryRemove(email, out _);
            return true;
        }
    }
}
