using CourseMarketplaceFE.Helpers;

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

            // Đăng ký ApiClient (auto-attach AccessToken + auto-refresh khi 401)
            builder.Services.AddScoped<ApiClient>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Kích hoạt xác thực và phân quyền (nếu dùng Identity ở FE)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
              name: "default",
               pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
