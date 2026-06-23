using System.Security.Claims;
using CourseMarketplaceFE.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CourseMarketplaceFE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔥 Stripe PublishableKey – đọc từ biến môi trường (Docker inject)
            var stripePublishableKey = Environment.GetEnvironmentVariable("Stripe__PublishableKey");
            if (!string.IsNullOrWhiteSpace(stripePublishableKey))
            {
                builder.Configuration["Stripe:PublishableKey"] = stripePublishableKey;
            }

            // 🔥 Google ClientId – đọc từ biến môi trường
            var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            if (!string.IsNullOrWhiteSpace(googleClientId))
            {
                builder.Configuration["Authentication:Google:ClientId"] = googleClientId;
            }

            // 1. Đăng ký HttpClient để FE có thể gọi API Backend
            builder.Services.AddHttpClient("BackendApi", client =>
            {
                var apiUrl = builder.Configuration.GetValue<string>("BackendApiUrl");
                client.BaseAddress = new Uri(apiUrl ?? "http://localhost:5207/api/");
            })
      .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
          // Tắt cookie tự động của HttpClientHandler (để ApiClient tự quản lý)
          UseCookies = false,
          // Bỏ qua check SSL lỗi trên localhost development
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      });

            // 2. Thêm dịch vụ HttpContextAccessor để truy cập Cookie dễ dàng hơn ở các lớp khác
            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // 3. Cấu hình Authentication để [Authorize] hoạt động
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.Cookie.Name = "LinkedLearn.Auth";
                });

            // Đăng ký ApiClient (auto-attach AccessToken + auto-refresh khi 401)
            builder.Services.AddScoped<ApiClient>();

            // --- CẤU HÌNH YARP (REVERSE PROXY) CHO SIGNALR & API ---
            var backendBaseUrl = builder.Configuration.GetValue<string>("BackendBaseUrl") ?? "http://localhost:5207";
            builder.Services.AddReverseProxy()
                .LoadFromMemory(new[]
                {
                    new Yarp.ReverseProxy.Configuration.RouteConfig
                    {
                        RouteId = "api_route",
                        ClusterId = "backend_cluster",
                        Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/api/{**catch-all}" }
                    },
                    new Yarp.ReverseProxy.Configuration.RouteConfig
                    {
                        RouteId = "chathub_route",
                        ClusterId = "backend_cluster",
                        Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/chatHub/{**catch-all}" }
                    },
                    new Yarp.ReverseProxy.Configuration.RouteConfig
                    {
                        RouteId = "notifhub_route",
                        ClusterId = "backend_cluster",
                        Match = new Yarp.ReverseProxy.Configuration.RouteMatch { Path = "/notificationHub/{**catch-all}" }
                    }
                }, new[]
                {
                    new Yarp.ReverseProxy.Configuration.ClusterConfig
                    {
                        ClusterId = "backend_cluster",
                        Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "backend_destination", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = backendBaseUrl } }
                        }
                    }
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Middleware ép buộc Force Logout khi API trả về 401 (token chết)
            app.Use(async (context, next) =>
            {
                await next();

                // Nếu ApiClient đánh dấu cần ForceLogout, xóa cookies hiển thị và redirect về Login
                if (context.Items.ContainsKey("ForceLogout") && !context.Response.HasStarted)
                {
                    context.Response.Cookies.Delete("AccessToken", new CookieOptions { Path = "/" });
                    context.Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/" });
                    context.Response.Cookies.Delete("UserName", new CookieOptions { Path = "/" });
                    context.Response.Cookies.Delete("AvatarUrl", new CookieOptions { Path = "/" });
                    context.Response.Cookies.Delete("UserRole", new CookieOptions { Path = "/" });
                    context.Response.Redirect("/Account/Login");
                }
            });

            app.UseRouting();

            app.UseAuthentication();

            // Middleware tự động map Cookies (UserRole, UserId) sang ClaimsPrincipal 
            // để [Authorize(Roles="...")] hoạt động mà không cần Identity phức tạp
            app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    var role = context.Request.Cookies["UserRole"];
                    var userId = context.Request.Cookies["UserId"];
                    var userName = context.Request.Cookies["UserName"];

                    if (!string.IsNullOrEmpty(role) && !string.IsNullOrEmpty(userId))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId),
                            new Claim(ClaimTypes.Role, role),
                            new Claim(ClaimTypes.Name, userName ?? "User")
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        context.User = new ClaimsPrincipal(identity);
                    }
                }

                await next();
            });

            app.UseAuthorization();

            app.MapReverseProxy();

            app.MapControllerRoute(
              name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
