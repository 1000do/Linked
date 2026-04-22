using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AiActivityLog> AiActivityLogs { get; set; }

    public virtual DbSet<AiModel> AiModels { get; set; }

    public virtual DbSet<AiModelsCourse> AiModelsCourses { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<EnrollmentProgress> EnrollmentProgresses { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<InstructorPayout> InstructorPayouts { get; set; }

    public virtual DbSet<LearningMaterial> LearningMaterials { get; set; }

    public virtual DbSet<MaterialPHash> MaterialPHashes { get; set; }

    public virtual DbSet<MaterialEmbedding> MaterialEmbeddings { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Manager> Managers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OrderInfo> OrderInfos { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserReport> UserReports { get; set; }

    public virtual DbSet<WishlistItem> WishlistItems { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Linked;Username=postgres;Password=123456");


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // If DbContextOptions were already configured by DI (Program.cs AddDbContext), do nothing.
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        // Allow environment variable override when running outside DI (or in tests).
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                   ?? "Host=localhost;Port=5432;Database=linked;Username=postgres;Password=123456";

        optionsBuilder.UseNpgsql(conn);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.HasIndex(e => e.Email, "accounts_email_key").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AccountCreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("account_created_at");
            entity.Property(e => e.AccountFlagCount)
                .HasDefaultValue(0)
                .HasColumnName("account_flag_count");
            entity.Property(e => e.AccountLastLoginAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("account_last_login_at");
            entity.Property(e => e.AccountStatus)
                .HasMaxLength(50)
                .HasColumnName("account_status");
            entity.Property(e => e.AccountUpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("account_updated_at");
            entity.Property(e => e.AuthProvider)
                .HasMaxLength(50)
                .HasColumnName("auth_provider");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .HasColumnName("phone_number");
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
            entity.Property(e => e.RefreshTokenExpiryTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("refresh_token_expiry_time");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
        });

        modelBuilder.Entity<AiActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("ai_activity_logs_pkey");

            entity.ToTable("ai_activity_logs");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.AiModelCourseId).HasColumnName("ai_model_course_id");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.InputJson)
                .HasColumnType("jsonb")
                .HasColumnName("input_json");
            entity.Property(e => e.InteractionType)
                .HasMaxLength(50)
                .HasColumnName("interaction_type");
            entity.Property(e => e.LatencyMs).HasColumnName("latency_ms");
            entity.Property(e => e.LogCreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("log_created_at");
            entity.Property(e => e.LogStatus)
                .HasMaxLength(50)
                .HasColumnName("log_status");
            entity.Property(e => e.OutputJson)
                .HasColumnType("jsonb")
                .HasColumnName("output_json");
            entity.Property(e => e.TokenUsage).HasColumnName("token_usage");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AiModelCourse).WithMany(p => p.AiActivityLogs)
                .HasForeignKey(d => d.AiModelCourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_activity_logs_ai_model_course_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.AiActivityLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_activity_logs_user_id_fkey");
        });

        modelBuilder.Entity<AiModel>(entity =>
        {
            entity.HasKey(e => e.ModelId).HasName("ai_models_pkey");

            entity.ToTable("ai_models");

            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ModelCreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("model_created_at");
            entity.Property(e => e.ModelName)
                .HasMaxLength(255)
                .HasColumnName("model_name");
            entity.Property(e => e.ModelProvider)
                .HasMaxLength(50)
                .HasColumnName("model_provider");
            entity.Property(e => e.ModelStatus)
                .HasMaxLength(50)
                .HasColumnName("model_status");
            entity.Property(e => e.ModelType)
                .HasMaxLength(50)
                .HasColumnName("model_type");
            entity.Property(e => e.ModelUpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("model_updated_at");
            entity.Property(e => e.ModelVersion)
                .HasMaxLength(50)
                .HasColumnName("model_version");
        });

        modelBuilder.Entity<AiModelsCourse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_models_courses_pkey");

            entity.ToTable("ai_models_courses");

            entity.HasIndex(e => new { e.ModelId, e.CourseId }, "ai_models_courses_model_id_course_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("assigned_at");
            entity.Property(e => e.ConfigJson)
                .HasColumnType("jsonb")
                .HasColumnName("config_json");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.IsEnabled)
                .HasDefaultValue(true)
                .HasColumnName("is_enabled");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");

            entity.HasOne(d => d.Course).WithMany(p => p.AiModelsCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_models_courses_course_id_fkey");

            entity.HasOne(d => d.Model).WithMany(p => p.AiModelsCourses)
                .HasForeignKey(d => d.ModelId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ai_models_courses_model_id_fkey");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cart_items_pkey");

            entity.ToTable("cart_items");

            entity.HasIndex(e => new { e.UserId, e.CourseId }, "cart_items_user_id_course_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("added_date");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("cart_items_course_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("cart_items_user_id_fkey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoriesName)
                .HasMaxLength(255)
                .HasColumnName("categories_name");
            entity.Property(e => e.CategoryStatus)
                .HasMaxLength(50)
                .HasColumnName("category_status");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("chats_pkey");

            entity.ToTable("chats");

            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.LastMessageAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_message_at");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("coupons_pkey");

            entity.ToTable("coupons");

            entity.HasIndex(e => e.CouponCode, "coupons_coupon_code_key").IsUnique();

            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(50)
                .HasColumnName("coupon_code");
            entity.Property(e => e.CouponType)
                .HasMaxLength(50)
                .HasColumnName("coupon_type");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(10, 2)
                .HasColumnName("discount_value");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsedCount)
                .HasDefaultValue(0)
                .HasColumnName("used_count");

            entity.HasOne(d => d.Manager).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("coupons_manager_id_fkey");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("courses_pkey");

            entity.ToTable("courses");

            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CourseFlagCount)
                .HasDefaultValue(0)
                .HasColumnName("course_flag_count");
            entity.Property(e => e.CourseStatus)
                .HasMaxLength(50)
                .HasColumnName("course_status");
            entity.Property(e => e.CourseThumbnailUrl).HasColumnName("course_thumbnail_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            //entity.Property(e => e.RatingAverage)
            //    .HasDefaultValueSql("0.0")
            //    .HasColumnName("rating_average");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            //entity.Property(e => e.TotalLessons)
            //    .HasDefaultValue(0)
            //    .HasColumnName("total_lessons");
            //entity.Property(e => e.TotalStudents)
            //    .HasDefaultValue(0)
            //    .HasColumnName("total_students");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("courses_category_id_fkey");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CouponId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("courses_coupon_id_fkey");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("courses_instructor_id_fkey");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("enrollments_pkey");

            entity.ToTable("enrollments");

            entity.HasIndex(e => new { e.UserId, e.CourseId }, "enrollments_user_id_course_id_key").IsUnique();

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.CompletedDate).HasColumnName("completed_date");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EnrollDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("enroll_date");
            entity.Property(e => e.EnrollmentStatus)
                .HasMaxLength(50)
                .HasColumnName("enrollment_status");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.LastAccessedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_accessed_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("enrollments_course_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("enrollments_user_id_fkey");
        });

        modelBuilder.Entity<EnrollmentProgress>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("enrollment_progress_pkey");

            entity.ToTable("enrollment_progress");

            entity.Property(e => e.EnrollmentId)
                .ValueGeneratedNever()
                .HasColumnName("enrollment_id");
            entity.Property(e => e.LearnedMaterialCount)
                .HasDefaultValue(0)
                .HasColumnName("learned_material_count");
            entity.Property(e => e.LastModifiedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_modified_at");

            entity.HasOne(d => d.Enrollment).WithOne(p => p.EnrollmentProgress)
                .HasForeignKey<EnrollmentProgress>(d => d.EnrollmentId)
                .HasConstraintName("enrollment_progress_enrollment_id_fkey");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstructorId).HasName("instructors_pkey");

            entity.ToTable("instructors");

            entity.Property(e => e.InstructorId)
                .ValueGeneratedNever()
                .HasColumnName("instructor_id");
            entity.Property(e => e.ChargesEnabled)
                .HasDefaultValue(false)
                .HasColumnName("charges_enabled");
            //entity.Property(e => e.InstructorRating)
            //    .HasDefaultValueSql("0.0")
            //    .HasColumnName("instructor_rating");
            entity.Property(e => e.PayoutsEnabled)
                .HasDefaultValue(false)
                .HasColumnName("payouts_enabled");
            entity.Property(e => e.StripeAccountId)
                .HasMaxLength(255)
                .HasColumnName("stripe_account_id");
            entity.Property(e => e.StripeOnboardingStatus)
                .HasMaxLength(50)
                .HasColumnName("stripe_onboarding_status");
            //entity.Property(e => e.TotalRevenue)
            //    .HasPrecision(12, 2)
            //    .HasDefaultValueSql("0.00")
            //    .HasColumnName("total_revenue");

            entity.HasOne(d => d.InstructorNavigation).WithOne(p => p.Instructor)
                .HasForeignKey<Instructor>(d => d.InstructorId)
                .HasConstraintName("instructors_instructor_id_fkey");
        });

        modelBuilder.Entity<MaterialPHash>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("material_p_hashes_pkey");

            entity.ToTable("material_p_hashes");

            entity.Property(e => e.MaterialId)
                .ValueGeneratedNever()
                .HasColumnName("material_id");
            entity.Property(e => e.PHash).HasColumnName("p_hash");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Material).WithOne(p => p.MaterialPHash)
                .HasForeignKey<MaterialPHash>(d => d.MaterialId)
                .HasConstraintName("material_p_hashes_material_id_fkey");
        });

        modelBuilder.Entity<MaterialEmbedding>(entity =>
        {
            entity.HasKey(e => e.EmbeddingId).HasName("material_embeddings_pkey");

            entity.ToTable("material_embeddings");

            entity.Property(e => e.EmbeddingId).HasColumnName("embedding_id");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.EmbeddingVector).HasColumnName("embedding_vector");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Material).WithMany(p => p.MaterialEmbeddings)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("material_embeddings_material_id_fkey");
        });

        modelBuilder.Entity<InstructorPayout>(entity =>
        {
            entity.HasKey(e => e.PayoutId).HasName("instructor_payouts_pkey");

            entity.ToTable("instructor_payouts");

            entity.Property(e => e.PayoutId).HasColumnName("payout_id");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.InstructorId).HasColumnName("instructor_id");
            entity.Property(e => e.PayoutAmount)
                .HasPrecision(10, 2)
                .HasColumnName("payout_amount");
            entity.Property(e => e.PayoutDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("payout_date");
            entity.Property(e => e.IsPaid)
                .HasDefaultValue(false)
                .HasColumnName("is_paid");

            entity.HasOne(d => d.Transaction).WithMany(p => p.InstructorPayouts)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("instructor_payouts_transaction_id_fkey");

            entity.HasOne(d => d.Instructor).WithMany(p => p.InstructorPayouts)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("instructor_payouts_instructor_id_fkey");
        });

        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("learning_materials_pkey");

            entity.ToTable("learning_materials");

            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LearningStatus)
                .HasMaxLength(50)
                .HasColumnName("learning_status");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.MaterialUrl).HasColumnName("material_url");
            entity.Property(e => e.MaterialMetadata)
                .HasColumnType("jsonb")
                .HasColumnName("material_metadata");
            entity.Property(e => e.MaterialHash)
                .HasMaxLength(32)
                .HasColumnName("material_hash");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LearningMaterials)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("learning_materials_lesson_id_fkey");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("lessons_pkey");

            entity.ToTable("lessons");

            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LessonStatus)
                .HasMaxLength(50)
                .HasColumnName("lesson_status");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lessons_course_id_fkey");
        });

        modelBuilder.Entity<Manager>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("managers_pkey");

            entity.ToTable("managers");

            entity.Property(e => e.ManagerId)
                .ValueGeneratedNever()
                .HasColumnName("manager_id");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(255)
                .HasColumnName("display_name");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");

            entity.HasOne(d => d.ManagerNavigation).WithOne(p => p.Manager)
                .HasForeignKey<Manager>(d => d.ManagerId)
                .HasConstraintName("managers_manager_id_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.IsSeen)
                .HasDefaultValue(false)
                .HasColumnName("is_seen");
            entity.Property(e => e.ReceivedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("received_at");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("messages_chat_id_fkey");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("messages_receiver_id_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("messages_sender_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.LinkAction).HasColumnName("link_action");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Receiver).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notifications_receiver_id_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.NotificationSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notifications_sender_id_fkey");
        });

        modelBuilder.Entity<OrderInfo>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("order_info_pkey");

            entity.ToTable("order_info");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            //entity.Property(e => e.DiscountAmount)
            //    .HasPrecision(10, 2)
            //    .HasDefaultValueSql("0.00")
            //    .HasColumnName("discount_amount");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasColumnName("order_status");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            //entity.Property(e => e.TotalAmount)
            //    .HasPrecision(10, 2)
            //    .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.OrderInfos)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("order_info_user_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CouponUsed)
                .HasDefaultValue(false)
                .HasColumnName("coupon_used");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PurchasePrice)
                .HasPrecision(10, 2)
                .HasColumnName("purchase_price");

            entity.HasOne(d => d.Course).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("order_items_course_id_fkey");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("order_items_order_id_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("reviews_pkey");

            entity.ToTable("reviews");

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.IsRemoved)
                .HasDefaultValue(false)
                .HasColumnName("is_removed");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("reviews_enrollment_id_fkey");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("system_configs_pkey");

            entity.ToTable("system_configs");

            entity.HasIndex(e => e.ConfigKey, "system_configs_config_key_key").IsUnique();

            entity.Property(e => e.ConfigId).HasColumnName("config_id");
            entity.Property(e => e.ConfigKey)
                .HasMaxLength(255)
                .HasColumnName("config_key");
            entity.Property(e => e.ConfigValue).HasColumnName("config_value");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Manager).WithMany(p => p.SystemConfigs)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("system_configs_manager_id_fkey");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.AccountFrom).HasColumnName("account_from");
            entity.Property(e => e.AccountTo).HasColumnName("account_to");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValueSql("'VND'::character varying")
                .HasColumnName("currency");
            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.StripePaymentintentId)
                .HasMaxLength(255)
                .HasColumnName("stripe_paymentintent_id");
            entity.Property(e => e.StripeSessionId)
                .HasMaxLength(255)
                .HasColumnName("stripe_session_id");
            entity.Property(e => e.TransactionCreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_created_at");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .HasColumnName("transaction_type");
            entity.Property(e => e.TransactionsStatus)
                .HasMaxLength(50)
                .HasColumnName("transactions_status");
            entity.Property(e => e.TransferRate)
                .HasPrecision(5, 2)
                .HasDefaultValue(100m)
                .HasColumnName("transfer_rate");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_order_item_id_fkey");

            entity.HasOne(d => d.AccountFromNavigation).WithMany(p => p.TransactionsFrom)
                .HasForeignKey(d => d.AccountFrom)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_account_from_fkey");

            entity.HasOne(d => d.AccountToNavigation).WithMany(p => p.TransactionsTo)
                .HasForeignKey(d => d.AccountTo)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_account_to_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            //entity.Property(e => e.EnrolledCoursesCount)
            //    .HasDefaultValue(0)
            //    .HasColumnName("enrolled_courses_count");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            //entity.Property(e => e.TotalSpent)
            //    .HasPrecision(12, 2)
            //    .HasDefaultValueSql("0.00")
            //    .HasColumnName("total_spent");

            entity.HasOne(d => d.UserNavigation).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.UserId)
                .HasConstraintName("users_user_id_fkey");
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("user_reports_pkey");

            entity.ToTable("user_reports");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.ReporterId).HasColumnName("reporter_id");
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.ResolvedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("resolved_at");
            entity.Property(e => e.ResolverId).HasColumnName("resolver_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.UserReportsStatus)
                .HasMaxLength(50)
                .HasColumnName("user_reports_status");

            entity.HasOne(d => d.Reporter).WithMany(p => p.UserReportReporters)
                .HasForeignKey(d => d.ReporterId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_reports_reporter_id_fkey");

            entity.HasOne(d => d.Resolver).WithMany(p => p.UserReportResolvers)
                .HasForeignKey(d => d.ResolverId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_reports_resolver_id_fkey");

            entity.HasOne(d => d.Target).WithMany(p => p.UserReportTargets)
                .HasForeignKey(d => d.TargetId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_reports_target_id_fkey");
        });

        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wishlist_items_pkey");

            entity.ToTable("wishlist_items");

            entity.HasIndex(e => new { e.UserId, e.CourseId }, "wishlist_items_user_id_course_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("added_date");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Course).WithMany(p => p.WishlistItems)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("wishlist_items_course_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.WishlistItems)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("wishlist_items_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
