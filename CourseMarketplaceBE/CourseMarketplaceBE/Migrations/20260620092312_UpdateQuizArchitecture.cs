using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CourseMarketplaceBE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuizArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThreatLevel",
                table: "courses",
                newName: "threat_level");

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "managers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bio",
                table: "managers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "managers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "managers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "instructors",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "threat_level",
                table: "courses",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "gifts",
                columns: table => new
                {
                    gift_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_item_id = table.Column<int>(type: "integer", nullable: false),
                    sender_id = table.Column<int>(type: "integer", nullable: true),
                    recipient_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    gift_message = table.Column<string>(type: "text", nullable: true),
                    card_theme = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "classic"),
                    redemption_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_claimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    claimed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    claimed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    delivery_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("gifts_pkey", x => x.gift_id);
                    table.ForeignKey(
                        name: "gifts_claimed_by_user_id_fkey",
                        column: x => x.claimed_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "gifts_order_item_id_fkey",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "gifts_sender_id_fkey",
                        column: x => x.sender_id,
                        principalTable: "accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    question_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    lesson_id = table.Column<int>(type: "integer", nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    question_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "SingleChoice"),
                    order_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("quiz_questions_pkey", x => x.question_id);
                    table.ForeignKey(
                        name: "quiz_questions_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quiz_questions_lesson_id_fkey",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "lesson_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    quiz_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    instructor_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    time_limit_minutes = table.Column<int>(type: "integer", nullable: true),
                    passing_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 70),
                    total_questions = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("quizzes_pkey", x => x.quiz_id);
                    table.ForeignKey(
                        name: "quizzes_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quizzes_instructor_id_fkey",
                        column: x => x.instructor_id,
                        principalTable: "instructors",
                        principalColumn: "instructor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_options",
                columns: table => new
                {
                    option_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    option_text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    order_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quiz_options_pkey", x => x.option_id);
                    table.ForeignKey(
                        name: "quiz_options_question_id_fkey",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_quizzes",
                columns: table => new
                {
                    course_quiz_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    quiz_id = table.Column<int>(type: "integer", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_hidden = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    added_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("course_quizzes_pkey", x => x.course_quiz_id);
                    table.ForeignKey(
                        name: "course_quizzes_course_id_fkey",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "course_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "course_quizzes_quiz_id_fkey",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    attempt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quiz_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: true),
                    is_passed = table.Column<bool>(type: "boolean", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    submitted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quiz_attempts_pkey", x => x.attempt_id);
                    table.ForeignKey(
                        name: "quiz_attempts_quiz_id_fkey",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quiz_attempts_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_lesson_distributions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quiz_id = table.Column<int>(type: "integer", nullable: false),
                    lesson_id = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quiz_lesson_distributions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "quiz_lesson_distributions_lesson_id_fkey",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "lesson_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quiz_lesson_distributions_quiz_id_fkey",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "quiz_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempt_answers",
                columns: table => new
                {
                    answer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attempt_id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    selected_option_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quiz_attempt_answers_pkey", x => x.answer_id);
                    table.ForeignKey(
                        name: "quiz_attempt_answers_attempt_id_fkey",
                        column: x => x.attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "attempt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quiz_attempt_answers_question_id_fkey",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quiz_attempt_answers_selected_option_id_fkey",
                        column: x => x.selected_option_id,
                        principalTable: "quiz_options",
                        principalColumn: "option_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempt_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    attempt_id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quiz_attempt_questions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "quiz_attempt_questions_attempt_id_fkey",
                        column: x => x.attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "attempt_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "quiz_attempt_questions_question_id_fkey",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_quizzes_quiz_id",
                table: "course_quizzes",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "uq_course_quiz",
                table: "course_quizzes",
                columns: new[] { "course_id", "quiz_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_gifts_delivery",
                table: "gifts",
                column: "delivery_status");

            migrationBuilder.CreateIndex(
                name: "idx_gifts_recipient",
                table: "gifts",
                column: "recipient_email");

            migrationBuilder.CreateIndex(
                name: "idx_gifts_token",
                table: "gifts",
                column: "redemption_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gifts_claimed_by_user_id",
                table: "gifts",
                column: "claimed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_gifts_order_item_id",
                table: "gifts",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_gifts_sender_id",
                table: "gifts",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempt_answers_attempt_id",
                table: "quiz_attempt_answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempt_answers_question_id",
                table: "quiz_attempt_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempt_answers_selected_option_id",
                table: "quiz_attempt_answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempt_questions_question_id",
                table: "quiz_attempt_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "uq_quiz_attempt_question",
                table: "quiz_attempt_questions",
                columns: new[] { "attempt_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_quiz_id",
                table: "quiz_attempts",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_user_id",
                table: "quiz_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_lesson_distributions_lesson_id",
                table: "quiz_lesson_distributions",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "uq_quiz_lesson_dist",
                table: "quiz_lesson_distributions",
                columns: new[] { "quiz_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_options_question_id",
                table: "quiz_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_course_id",
                table: "quiz_questions",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_lesson_id",
                table: "quiz_questions",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_course_id",
                table: "quizzes",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_quizzes_instructor_id",
                table: "quizzes",
                column: "instructor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_quizzes");

            migrationBuilder.DropTable(
                name: "gifts");

            migrationBuilder.DropTable(
                name: "quiz_attempt_answers");

            migrationBuilder.DropTable(
                name: "quiz_attempt_questions");

            migrationBuilder.DropTable(
                name: "quiz_lesson_distributions");

            migrationBuilder.DropTable(
                name: "quiz_options");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "managers");

            migrationBuilder.DropColumn(
                name: "bio",
                table: "managers");

            migrationBuilder.DropColumn(
                name: "full_name",
                table: "managers");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "managers");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "instructors");

            migrationBuilder.RenameColumn(
                name: "threat_level",
                table: "courses",
                newName: "ThreatLevel");

            migrationBuilder.AlterColumn<int>(
                name: "ThreatLevel",
                table: "courses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }
    }
}
