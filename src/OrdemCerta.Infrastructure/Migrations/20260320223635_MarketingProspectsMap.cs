using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrdemCerta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarketingProspectsMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "marketing_prospects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    place_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    business_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    state = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    contacted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketing_prospects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_marketing_prospects_place_id",
                table: "marketing_prospects",
                column: "place_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "marketing_prospects");
        }
    }
}
