using Google.Apis.Auth;

namespace CourseMarketplaceBE.Application.IServices;

public interface IGoogleTokenValidator
{
    Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
}
