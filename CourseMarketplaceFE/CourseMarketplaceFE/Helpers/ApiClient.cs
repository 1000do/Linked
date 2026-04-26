using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CourseMarketplaceFE.Helpers
{
    /// <summary>
    /// Wrapper quanh IHttpClientFactory.
    /// - Tự động gắn Bearer AccessToken lấy từ cookie vào mỗi request.
    /// - Khi nhận 401, tự gọi /api/auth/refresh để lấy token mới rồi thử lại 1 lần.
    /// - Nếu refresh cũng thất bại → trả về 401 để Controller redirect về Login.
    /// </summary>
    public class ApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;
        private readonly JsonSerializerOptions _jsonOpts =
            new() { PropertyNameCaseInsensitive = true };

        public ApiClient(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {
            _factory = factory;
            _ctx = ctx;
        }

        // ─── Public helpers ───────────────────────────────────────────────

        public Task<HttpResponseMessage> GetAsync(string url)
            => SendWithRetryAsync(() => BuildRequest(HttpMethod.Get, url));

        public Task<HttpResponseMessage> PostJsonAsync<T>(string url, T body)
            => SendWithRetryAsync(() => BuildJsonRequest(HttpMethod.Post, url, body));

        public Task<HttpResponseMessage> PatchJsonAsync<T>(string url, T body)
            => SendWithRetryAsync(() => BuildJsonRequest(HttpMethod.Patch, url, body));

        public Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
            => SendWithRetryAsync(() => BuildRawRequest(HttpMethod.Put, url, content));

        public Task<HttpResponseMessage> PostAsync(string url)
            => SendWithRetryAsync(() => BuildRequest(HttpMethod.Post, url));

        public Task<HttpResponseMessage> PostFormDataAsync(string url, MultipartFormDataContent content)
            => SendWithRetryAsync(() => BuildRawRequest(HttpMethod.Post, url, content));

        public Task<HttpResponseMessage> DeleteAsync(string url)
            => SendWithRetryAsync(() => BuildRequest(HttpMethod.Delete, url));

        // ─── Core logic ───────────────────────────────────────────────────

        /// <summary>
        /// Gửi request; nếu 401 → thử refresh token rồi gửi lại 1 lần duy nhất.
        /// </summary>
        private async Task<HttpResponseMessage> SendWithRetryAsync(
            Func<HttpRequestMessage> requestFactory)
        {
            var response = await SendAsync(requestFactory());

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Cố gắng refresh
                var refreshed = await TryRefreshTokenAsync();
                if (refreshed)
                {
                    response = await SendAsync(requestFactory()); // thử lại với token mới
                }

                // Nếu vẫn 401 (nghĩa là refresh thất bại, hoặc token mới vẫn bị từ chối)
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var httpCtx = _ctx.HttpContext;
                    if (httpCtx != null)
                    {
                        // Xóa sạch session/cookie
                        httpCtx.Response.Cookies.Delete("AccessToken");
                        httpCtx.Response.Cookies.Delete("RefreshToken");
                        httpCtx.Response.Cookies.Delete("UserRole");

                        // Ra hiệu cho Middleware bên Program.cs đá văng về trang đăng nhập
                        httpCtx.Items["ForceLogout"] = true;
                    }
                }
            }

            return response;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var client = _factory.CreateClient("BackendApi");
            return await client.SendAsync(request);
        }

        // ─── Token refresh ────────────────────────────────────────────────

        /// <summary>
        /// Gọi POST /api/auth/refresh kèm RefreshToken cookie.
        /// Nếu thành công → Set-Cookie AccessToken mới lên response FE.
        /// </summary>
        private async Task<bool> TryRefreshTokenAsync()
        {
            var httpCtx = _ctx.HttpContext;
            if (httpCtx == null) return false;

            var refreshToken = httpCtx.Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var client = _factory.CreateClient("BackendApi");

            // Gửi refresh token qua header X-Refresh-Token (BE chấp nhận cả cookie lẫn header)
            var req = new HttpRequestMessage(HttpMethod.Post, "auth/refresh");
            req.Headers.Add("X-Refresh-Token", refreshToken);

            var resp = await client.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return false;

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("accessToken", out var atEl)) return false;
            var newAccessToken = atEl.GetString();
            if (string.IsNullOrEmpty(newAccessToken)) return false;

            // Lưu AccessToken mới vào cookie FE
            httpCtx.Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = httpCtx.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                Path = "/"
            });

            // Nếu BE trả về refreshToken mới (rotation), lưu luôn
            if (root.TryGetProperty("refreshToken", out var rtEl))
            {
                var newRefreshToken = rtEl.GetString();
                if (!string.IsNullOrEmpty(newRefreshToken))
                {
                    httpCtx.Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = httpCtx.Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        Path = "/"
                    });
                }
            }

            return true;
        }

        // ─── Request builders ─────────────────────────────────────────────

        private HttpRequestMessage BuildRequest(HttpMethod method, string url)
        {
            var req = new HttpRequestMessage(method, url);
            AttachAccessToken(req);
            return req;
        }

        private HttpRequestMessage BuildJsonRequest<T>(HttpMethod method, string url, T body)
        {
            var req = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(body)
            };
            AttachAccessToken(req);
            return req;
        }

        private HttpRequestMessage BuildRawRequest(HttpMethod method, string url, HttpContent content)
        {
            var req = new HttpRequestMessage(method, url) { Content = content };
            AttachAccessToken(req);
            return req;
        }

        private void AttachAccessToken(HttpRequestMessage req)
        {
            var token = _ctx.HttpContext?.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
