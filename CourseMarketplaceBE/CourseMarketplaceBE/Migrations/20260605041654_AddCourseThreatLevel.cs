using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace CourseMarketplaceBE.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseThreatLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "material_embeddings");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<int>(
                name: "ThreatLevel",
                table: "courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "process_type",
                table: "ai_models",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "accounts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "media_embeddings",
                columns: table => new
                {
                    media_embedding_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_id = table.Column<int>(type: "integer", nullable: true),
                    media_embedding = table.Column<Vector>(type: "vector(512)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("media_embeddings_pkey", x => x.media_embedding_id);
                    table.ForeignKey(
                        name: "media_embeddings_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "learning_materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "text_embeddings",
                columns: table => new
                {
                    text_embedding_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_id = table.Column<int>(type: "integer", nullable: true),
                    text_embedding = table.Column<Vector>(type: "vector(768)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("text_embeddings_pkey", x => x.text_embedding_id);
                    table.ForeignKey(
                        name: "text_embeddings_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "learning_materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "course_exts_description_hash_key",
                table: "course_exts",
                column: "description_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "course_exts_requirements_hash_key",
                table: "course_exts",
                column: "requirements_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "course_exts_thumbnail_hash_key",
                table: "course_exts",
                column: "thumbnail_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "course_exts_title_hash_key",
                table: "course_exts",
                column: "title_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "course_exts_what_you_will_learn_hash_key",
                table: "course_exts",
                column: "what_you_will_learn_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "accounts_username_key",
                table: "accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_embeddings_material_id",
                table: "media_embeddings",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_text_embeddings_material_id",
                table: "text_embeddings",
                column: "material_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_embeddings");

            migrationBuilder.DropTable(
                name: "text_embeddings");

            migrationBuilder.DropIndex(
                name: "course_exts_description_hash_key",
                table: "course_exts");

            migrationBuilder.DropIndex(
                name: "course_exts_requirements_hash_key",
                table: "course_exts");

            migrationBuilder.DropIndex(
                name: "course_exts_thumbnail_hash_key",
                table: "course_exts");

            migrationBuilder.DropIndex(
                name: "course_exts_title_hash_key",
                table: "course_exts");

            migrationBuilder.DropIndex(
                name: "course_exts_what_you_will_learn_hash_key",
                table: "course_exts");

            migrationBuilder.DropIndex(
                name: "accounts_username_key",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "ThreatLevel",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "process_type",
                table: "ai_models");

            migrationBuilder.DropColumn(
                name: "username",
                table: "accounts");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "material_embeddings",
                columns: table => new
                {
                    embedding_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    material_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    embedding = table.Column<string>(type: "vector(768)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("material_embeddings_pkey", x => x.embedding_id);
                    table.ForeignKey(
                        name: "material_embeddings_material_id_fkey",
                        column: x => x.material_id,
                        principalTable: "learning_materials",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_material_embeddings_material_id",
                table: "material_embeddings",
                column: "material_id");
        }
    }
}
