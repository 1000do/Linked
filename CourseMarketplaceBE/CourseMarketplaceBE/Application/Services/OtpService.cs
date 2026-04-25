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

        public string GenerateOtp(string email, string purpose)
        {
            var key = $"{email}_{purpose}";

            var otp = new Random().Next(100000, 999999).ToString();

            _store[key] = new OtpInfo
            {
                Code = otp,
                ExpireAt = DateTime.UtcNow.AddMinutes(2)
            };

            return otp;
        }

        // CHỈ CHECK - KHÔNG XOÁ
        public bool ValidateOtp(string email, string otp, string purpose)
        {
            var key = $"{email}_{purpose}";

            if (!_store.TryGetValue(key, out var info))
                return false;

            if (DateTime.UtcNow > info.ExpireAt)
            {
                _store.TryRemove(key, out _);
                return false;
            }

            return info.Code == otp;
        }

        //  CHECK + XOÁ
        public bool ConsumeOtp(string email, string otp, string purpose)
        {
            var key = $"{email}_{purpose}";

            if (!_store.TryGetValue(key, out var info))
                return false;

            if (DateTime.UtcNow > info.ExpireAt)
            {
                _store.TryRemove(key, out _);
                return false;
            }

            if (info.Code != otp)
                return false;

            _store.TryRemove(key, out _);
            return true;
        }
    }
}
