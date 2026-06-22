using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace CourseMarketplaceBE.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTextEmbeddingDimension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThreatLevel",
                table: "courses",
                newName: "threat_level");

            migrationBuilder.Sql("TRUNCATE TABLE text_embeddings;");

            migrationBuilder.AlterColumn<Vector>(
                name: "text_embedding",
                table: "text_embeddings",
                type: "vector(384)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(768)",
                oldNullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gifts");

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

            migrationBuilder.AlterColumn<Vector>(
                name: "text_embedding",
                table: "text_embeddings",
                type: "vector(768)",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(384)",
                oldNullable: true);

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
