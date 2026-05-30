using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CourseMarketplaceBE.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEnrollmentProgress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS enrollment_progress CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "course_ai_usage_logs_integration_id_fkey",
                table: "course_ai_usage_logs");

            migrationBuilder.DropTable(
                name: "course_reports");

            migrationBuilder.DropTable(
                name: "course_review_reports");

            migrationBuilder.DropTable(
                name: "courses_ai_integrations");

            migrationBuilder.DropTable(
                name: "lesson_review_reports");

            migrationBuilder.DropTable(
                name: "lockouts");

            migrationBuilder.DropTable(
                name: "material_completions");

            migrationBuilder.DropTable(
                name: "transaction_exts");

            migrationBuilder.DropTable(
                name: "user_avatar_frames");

            migrationBuilder.DropTable(
                name: "avatar_frames");

            migrationBuilder.DropColumn(
                name: "coupon_code",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "coupon_type",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "original_price",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "is_removed",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "cloud_public_id",
                table: "learning_materials");

            migrationBuilder.DropColumn(
                name: "facebook_url",
                table: "instructors");

            migrationBuilder.DropColumn(
                name: "youtube_url",
                table: "instructors");

            migrationBuilder.DropColumn(
                name: "is_removed",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "last_approved_at",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "requirements_hash",
                table: "course_exts");

            migrationBuilder.DropColumn(
                name: "what_you_will_learn_hash",
                table: "course_exts");

            migrationBuilder.DropColumn(
                name: "cleared_at",
                table: "chat_participants");

            migrationBuilder.DropColumn(
                name: "model_path",
                table: "ai_models");

            migrationBuilder.RenameColumn(
                name: "integration_id",
                table: "course_ai_usage_logs",
                newName: "ai_model_course_id");

            migrationBuilder.RenameIndex(
                name: "IX_course_ai_usage_logs_integration_id",
                table: "course_ai_usage_logs",
                newName: "IX_course_ai_usage_logs_ai_model_course_id");

            migrationBuilder.AddColumn<string>(
                name: "log_status",
                table: "message_moderation_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "message_id",
                table: "message_attachments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "log_status",
                table: "lesson_review_moderation_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "material_hash",
                table: "learning_materials",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "log_status",
                table: "course_review_moderation_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "log_status",
                table: "course_ai_usage_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ai_models_courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_id = table.Column<int>(type: "integer", nullable: true),
                    model_id = table.Column<int>(type: "integer", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    config_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_models_courses_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ai_models_courses_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "ai_models_courses_model_id_fkey",
                        column: x => x.model_id,
                        principalTable: "ai_models",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "enrollment_progress",
                columns: table => new
                {
                    enrollment_id = table.Column<int>(type: "integer", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    learned_material_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("enrollment_progress_pkey", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "enrollment_progress_enrollment_id_fkey",
                        column: x => x.enrollment_id,
                        principalTable: "enrollments",
                        principalColumn: "enrollment_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_exts",
                columns: table => new
                {
                    lesson_id = table.Column<int>(type: "integer", nullable: false),
                    description_hash = table.Column<string>(type: "character(32)", fixedLength: true, maxLength: 32, nullable: true),
                    thumbnail_hash = table.Column<string>(type: "character(32)", fixedLength: true, maxLength: 32, nullable: true),
                    title_hash = table.Column<string>(type: "character(32)", fixedLength: true, maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("lesson_exts_pkey", x => x.lesson_id);
                    table.ForeignKey(
                        name: "lesson_exts_lesson_id_fkey",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "lesson_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ai_models_courses_model_id_course_id_key",
                table: "ai_models_courses",
                columns: new[] { "model_id", "course_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_models_courses_course_id",
                table: "ai_models_courses",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "course_ai_usage_logs_ai_model_course_id_fkey",
                table: "course_ai_usage_logs",
                column: "ai_model_course_id",
                principalTable: "ai_models_courses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
