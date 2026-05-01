using System.Text;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using CourseMarketplaceBE.Infrastructure.Data;
using CourseMarketplaceBE.Infrastructure.Repositories;
using CourseMarketplaceBE.Infrastructure.Services;
using CourseMarketplaceBE.Share.Helpers;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;


namespace CourseMarketplaceBE;

public class Program
{
    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var builder = WebApplication.CreateBuilder(args);

        // 🔥 1. LOAD .env (chỉ khi chạy locally, docker compose sẽ skip)
        if (!builder.Environment.IsProduction())
        {
            Env.Load();
        }

        // 🔥 2. MAP .env / Docker environment → IConfiguration
        var envHost = Environment.GetEnvironmentVariable("DB_HOST")
                      ?? builder.Configuration["DB_HOST"];
        var envPort = Environment.GetEnvironmentVariable("DB_PORT")
                      ?? builder.Configuration["DB_PORT"];
        var envName = Environment.GetEnvironmentVariable("DB_NAME")
                      ?? builder.Configuration["DB_NAME"];
        var envUser = Environment.GetEnvironmentVariable("DB_USER")
                      ?? builder.Configuration["DB_USER"];
        var envPass = Environment.GetEnvironmentVariable("DB_PASSWORD")
                      ?? builder.Configuration["DB_PASSWORD"];

        string? builtConnectionString = null;
        if (!string.IsNullOrWhiteSpace(envHost))
        {
            // Use provided env vars (port may be empty; fallback to 5432)
            var port = string.IsNullOrWhiteSpace(envPort) ? "5432" : envPort;
            builtConnectionString =
                $"Host={envHost};Port={port};Database={envName ?? ""};Username={envUser ?? ""};Password={envPass ?? ""}";
        }

        // If we couldn't build from individual env vars, try other possible sources
        var fallbackFromConfig = builder.Configuration["ConnectionStrings:DefaultConnection"];
        var fallbackFromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        var finalConnectionString = builtConnectionString
                                    ?? fallbackFromConfig
                                    ?? fallbackFromEnv;

        if (string.IsNullOrWhiteSpace(finalConnectionString) || !finalConnectionString.Contains("Host="))
        {
            // Fail fast with a clear message instead of letting Npgsql throw later with "Host can't be null".
            throw new InvalidOperationException(
                "Database connection string is not configured. Set DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASSWORD or ConnectionStrings:DefaultConnection.");
        }

        // Ensure configuration has the connection string for places that read IConfiguration.
        builder.Configuration["ConnectionStrings:DefaultConnection"] = finalConnectionString;

        builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
        builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
        builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        builder.Configuration["Jwt:DurationInMinutes"] = Environment.GetEnvironmentVariable("JWT_DURATION");

        builder.Configuration["CloudinarySettings:CloudName"] =
            Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");

        builder.Configuration["CloudinarySettings:ApiKey"] =
            Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");

        builder.Configuration["CloudinarySettings:ApiSecret"] =
            Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        builder.Configuration["EmailSettings:Host"] =
    Environment.GetEnvironmentVariable("EMAIL_HOST");

        builder.Configuration["EmailSettings:Port"] =
            Environment.GetEnvironmentVariable("EMAIL_PORT");

        builder.Configuration["EmailSettings:EnableSSL"] =
            Environment.GetEnvironmentVariable("EMAIL_ENABLESSL");

        builder.Configuration["EmailSettings:Email"] =
            Environment.GetEnvironmentVariable("EMAIL_EMAIL");

        builder.Configuration["EmailSettings:Password"] =
            Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

        builder.Configuration["Authentication:Google:ClientId"] =
    Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

        var configuration = builder.Configuration;

        // 🔥 Stripe Configuration – đọc Secret Key từ biến môi trường (Docker inject)
        var stripeSecretKey = Environment.GetEnvironmentVariable("Stripe__SecretKey")
                              ?? configuration["Stripe:SecretKey"];
        if (!string.IsNullOrWhiteSpace(stripeSecretKey))
        {
            StripeConfiguration.ApiKey = stripeSecretKey;
            Console.WriteLine("✅ Stripe API Key đã được cấu hình.");
        }
        else
        {
            Console.WriteLine("⚠️  Warning: Stripe Secret Key chưa được cấu hình. Tính năng thanh toán sẽ không hoạt động.");
        }

        // 🔥 3. JWT Settings
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        builder.Services.AddSingleton(jwtSettings);

        // 🔥 4. Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dataSource));

        // 🔥 5. DI
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ICourseRepository, CourseRepository>();
        builder.Services.AddScoped<ILessonRepository, LessonRepository>();
        builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<ILessonService, LessonService>();
        builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
        builder.Services.AddScoped<IReviewService, CourseMarketplaceBE.Application.Services.ReviewService>();
        builder.Services.AddScoped<ILandingPageService, LandingPageService>();

        builder.Services.AddScoped<IChatRepository, ChatRepository>();
        builder.Services.AddSignalR(); // Đăng ký SignalR
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

        builder.Services.AddSingleton<IOtpService, OtpService>();
        builder.Services.AddScoped<IEmailService, EmailService>();

        builder.Services.AddScoped<IInstructorRepository, InstructorRepository>();
        builder.Services.AddScoped<IInstructorApprovalService, InstructorApprovalService>();

        builder.Services.AddScoped<ICouponRepository, CouponRepository>();
        builder.Services.AddScoped<ICouponService, CourseMarketplaceBE.Application.Services.CouponService>();

        // Register file upload implementation conditionally.
        // If Cloudinary config is present, use CloudinaryUploadService; otherwise use a no-op fallback.
        var cloudName = configuration["CloudinarySettings:CloudName"];
        var cloudApiKey = configuration["CloudinarySettings:ApiKey"];
        var cloudApiSecret = configuration["CloudinarySettings:ApiSecret"];

        if (!string.IsNullOrWhiteSpace(cloudName)
            && !string.IsNullOrWhiteSpace(cloudApiKey)
            && !string.IsNullOrWhiteSpace(cloudApiSecret))
        {
            builder.Services.AddScoped<IFileUploadService, CloudinaryUploadService>();
        }
        else
        {
            // Running without Cloudinary configured — register a safe fallback that returns nulls.
            Console.WriteLine("Warning: Cloudinary is not configured. File uploads will be no-ops.");
            builder.Services.AddScoped<IFileUploadService, NoopFileUploadService>();
        }

        builder.Services.AddScoped<IUserProfileService, UserProfileService>();
        builder.Services.AddScoped<IInstructorService, InstructorService>();

        // 🛒 Cart & Coupon
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<ICartService, CartService>();

        // 💳 Checkout & Payment (UC-19)
        builder.Services.AddScoped<ICheckoutRepository, CheckoutRepository>();
        builder.Services.AddScoped<ICheckoutService, CourseMarketplaceBE.Application.Services.CheckoutService>();
        builder.Services.AddScoped<IPaymentGatewayService, StripePaymentService>();
        // OCP: Đổi sang VNPay chỉ cần tạo VNPayPaymentService và đổi dòng trên.

        // 💰 Admin Finance (UC-112, UC-120)
        builder.Services.AddScoped<IAdminFinanceRepository, AdminFinanceRepository>();
        builder.Services.AddScoped<IAdminFinanceService, AdminFinanceService>();

        // 📊 Transactions (UC-114, UC-115)
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<IChatService, ChatService>();
        builder.Services.AddScoped<IModerationService, ModerationService>();

        builder.Services.AddHttpClient();

        // 🔥 6. Authentication
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
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key ?? "Default_Key_32_Chars_Minimum"))
            };

            //    options.Events = new JwtBearerEvents
            //    {
            //        OnMessageReceived = context =>
            //        {
            //            var cookieToken = context.Request.Cookies["AuthToken"];
            //            if (!string.IsNullOrEmpty(cookieToken))
            //                context.Token = cookieToken;

            //            return Task.CompletedTask;
            //        }
            //    };
            //});

            // Trong Program.cs, phần .AddJwtBearer
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // 1. Ưu tiên lấy Token từ Header Authorization (Swagger/Postman)
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authHeader)) return Task.CompletedTask;

                    // 2. Nếu là SignalR, nó thường gửi token qua query string "access_token"
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && 
                        (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/chatHub")))
                    {
                        context.Token = accessToken;
                        return Task.CompletedTask;
                    }

                    // 3. Cuối cùng, lấy từ Cookie (PHẢI KHỚP TÊN "AccessToken")
                    var cookieToken = context.Request.Cookies["AccessToken"]; // Sửa từ AuthToken -> AccessToken
                    if (!string.IsNullOrEmpty(cookieToken))
                        context.Token = cookieToken;

                    return Task.CompletedTask;
                }
            };
        });
        // 🔥 7. Controllers + Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Course Marketplace API",
                Version = "v1"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Nhập JWT token",
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
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
            });
        });

        // 🔥 8. CORS — cho phép FE MVC gọi BE API (dev: allow all origins)
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin()   // Cho phép mọi origin (FE port bất kỳ)
                      .AllowAnyMethod()
                      .AllowAnyHeader());
            // NOTE: AllowAnyOrigin() không tương thích với AllowCredentials().
            // Cookie/JWT vẫn hoạt động vì FE truyền Bearer Token qua Authorization header,
            // không phụ thuộc vào CORS credentials.
        });

        var app = builder.Build();

        // 🔥 9. Migration
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
                logger.LogError(ex, "Migration error");
            }
        }

        // 🔥 10. Middleware
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
            c.RoutePrefix = "swagger";
        });

        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHub<NotificationHub>("/notificationHub");
        app.MapHub<ChatHub>("/chatHub");

        app.MapControllers();

        app.Run();
    }
}