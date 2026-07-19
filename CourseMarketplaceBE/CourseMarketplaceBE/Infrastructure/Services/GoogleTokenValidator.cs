using CourseMarketplaceBE.Application.IServices;
using Google.Apis.Auth;

namespace CourseMarketplaceBE.Infrastructure.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    public async Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            IssuedAtClockTolerance = TimeSpan.FromMinutes(5)
        };
        return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }
}
