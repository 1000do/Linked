using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Application.IServices;

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

        // 2. Cấu hình Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 3. DI
        builder.Services.AddScoped<IUserRepository, UserRepository>();



        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IFileUploadService, CloudinaryUploadService>();
        builder.Services.AddScoped<IUserProfileService, UserProfileService>();

        builder.Services.AddHttpClient();

        // 4. Authentication & JWT Bearer
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
                    // Đọc từ Cookie trước, nếu không có thì Swagger Bearer sẽ tự động điền từ Header
                    var cookieToken = context.Request.Cookies["AuthToken"];
                    if (!string.IsNullOrEmpty(cookieToken)) context.Token = cookieToken;
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // 5. Swagger Authorize
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Course Marketplace API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Dán Token của bạn vào đây (Chỉ cần chuỗi mã).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] { }
                }
            });
        });

        // 6. CORS
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowAll", b => b.WithOrigins("http://localhost:5207")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
        });

        var app = builder.Build();

        // 7. Migration
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Migration error.");
            }
        }

        // 8. Middleware
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
            c.RoutePrefix = "swagger";
        });

        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}