using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace CourseMarketplaceBE.Presentation.Middlewares
{
    public class CookieObfuscationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDataProtector _protector;
        private const string ObfuscatedCookieName = "LinkedLearn.BE.Identity";
        
        // List of cookie names we want to hide
        private readonly string[] _targetCookies = new[] 
        { 
            "AccessToken", "RefreshToken" 
        };

        public CookieObfuscationMiddleware(RequestDelegate next, IDataProtectionProvider dataProtectionProvider)
        {
            _next = next;
            _protector = dataProtectionProvider.CreateProtector("CookieObfuscation");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. INCOMING REQUEST: Read the obfuscated cookie and restore the plain cookies into the request
            var obfuscatedCookie = context.Request.Cookies[ObfuscatedCookieName];
            var restoredCookies = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(obfuscatedCookie))
            {
                try
                {
                    var decrypted = _protector.Unprotect(obfuscatedCookie);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(decrypted);
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            restoredCookies[kvp.Key] = kvp.Value;
                        }

                        // Inject the restored cookies into the Cookie header so they are available to Request.Cookies
                        var cookieHeader = context.Request.Headers["Cookie"].ToString();
                        var newCookieHeader = string.IsNullOrEmpty(cookieHeader) ? "" : cookieHeader;

                        foreach (var kvp in restoredCookies)
                        {
                            var cookiePair = $"{kvp.Key}={kvp.Value}";
                            if (!string.IsNullOrEmpty(newCookieHeader))
                            {
                                newCookieHeader += $"; {cookiePair}";
                            }
                            else
                            {
                                newCookieHeader = cookiePair;
                            }
                        }

                        context.Request.Headers["Cookie"] = newCookieHeader;
                        
                        // Force the framework to re-parse the modified Cookie header
                        context.Features.Set<Microsoft.AspNetCore.Http.Features.IRequestCookiesFeature>(null);
                    }
                }
                catch
                {
                    // If decryption fails (e.g., keys rotated), clear the cookie
                    context.Response.Cookies.Delete(ObfuscatedCookieName);
                }
            }

            // 2. OUTGOING RESPONSE: Intercept Set-Cookie to capture target cookies and obfuscate them
            context.Response.OnStarting(() =>
            {
                var setCookieHeaders = context.Response.Headers["Set-Cookie"];
                if (setCookieHeaders.Count == 0)
                {
                    return Task.CompletedTask;
                }

                var updatedSetCookieHeaders = new List<string>();
                var newRestoredCookies = new Dictionary<string, string>(restoredCookies);
                bool cookieChanged = false;

                foreach (var header in setCookieHeaders)
                {
                    var parts = header.Split(';', StringSplitOptions.TrimEntries);
                    if (parts.Length == 0) continue;

                    var nameValue = parts[0].Split('=', 2);
                    if (nameValue.Length < 1) continue;

                    var cookieName = nameValue[0];
                    var cookieValue = nameValue.Length > 1 ? nameValue[1] : string.Empty;

                    if (_targetCookies.Contains(cookieName))
                    {
                        cookieChanged = true;
                        
                        // Check if it's a delete (usually value is empty or deleted)
                        if (string.IsNullOrEmpty(cookieValue) || cookieValue == "deleted")
                        {
                            if (newRestoredCookies.ContainsKey(cookieName))
                            {
                                newRestoredCookies.Remove(cookieName);
                            }
                        }
                        else
                        {
                            newRestoredCookies[cookieName] = cookieValue;
                        }
                        // Do NOT add this plain cookie header to updatedSetCookieHeaders (which hides it from the browser)
                    }
                    else
                    {
                        updatedSetCookieHeaders.Add(header);
                    }
                }

                if (cookieChanged)
                {
                    // Apply updated headers back
                    context.Response.Headers["Set-Cookie"] = updatedSetCookieHeaders.ToArray();

                    if (newRestoredCookies.Count > 0)
                    {
                        // Serialize and encrypt
                        var json = JsonSerializer.Serialize(newRestoredCookies);
                        var encrypted = _protector.Protect(json);

                        // Set the obfuscated cookie
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = context.Request.IsHttps,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(7), // Typically matches refresh token lifespan
                            Path = "/"
                        };
                        context.Response.Cookies.Append(ObfuscatedCookieName, encrypted, cookieOptions);
                    }
                    else
                    {
                        // All target cookies were deleted, so delete the obfuscated cookie too
                        context.Response.Cookies.Delete(ObfuscatedCookieName, new CookieOptions { Path = "/" });
                    }
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
