namespace CourseMarketplaceFE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Đăng ký HttpClient để FE có thể gọi API Backend
            builder.Services.AddHttpClient("BackendApi", client =>
            {
                var apiUrl = builder.Configuration.GetValue<string>("BackendApiUrl");
                client.BaseAddress = new Uri(apiUrl ?? "http://localhost:5207/api/");
            })
      .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
          // Cho phép HttpClient tự động xử lý Cookie từ Server trả về
          UseCookies = true,
          CookieContainer = new System.Net.CookieContainer(),
          // Nếu bạn dùng https trên localhost có thể cần dòng này để bỏ qua check SSL lỗi
          ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
      });
            // 2. Thêm dịch vụ HttpContextAccessor để truy cập Cookie dễ dàng hơn ở các lớp khác
            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

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
