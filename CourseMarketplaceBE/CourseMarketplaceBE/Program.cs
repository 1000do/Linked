using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Infrastructure.Repositories;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Share.Helpers;

namespace CourseMarketplaceBE;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        // 1. Cấu hình JWT
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        builder.Services.AddSingleton(jwtSettings);

        // 2. Cấu hình Database (Kết nối tới Docker Container 'db')
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 3. Đăng ký DI (Dependency Injection)
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();

        // 4. Cấu hình Authentication & JWT Bearer
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key ?? "Key_Mac_Dinh_Sieu_Bao_Mat_32_Ky_Tu")),
                ValidateIssuerSigningKey = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["AuthToken"];
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // 5. Cấu hình CORS (Cho phép Frontend kết nối)
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowAll", b => b.WithOrigins("http://localhost:5207")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
        });

        var app = builder.Build();

        // 6. TỰ ĐỘNG TẠO BẢNG DATABASE (QUAN TRỌNG NHẤT)
        // Đoạn này giúp giải quyết lỗi 500 khi DB chưa có bảng Users
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Lỗi xảy ra khi Migration dữ liệu trong Docker.");
            }
        }

        // 7. Cấu hình Middleware Pipeline
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Course Marketplace API V1");
            c.RoutePrefix = "swagger";
        });

        app.UseCors("AllowAll");

        // Tắt HttpsRedirection để tránh lỗi SSL khi chạy local Docker
        // app.UseHttpsRedirection(); 

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}