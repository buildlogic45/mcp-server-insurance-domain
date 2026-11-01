using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildLogic45.Insurance.Infrastructure.Persistence.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, collation: "C"),
                    policy_holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    policy_holder_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expiration_date = table.Column<DateOnly>(type: "date", nullable: false),
                    premium = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    coverage_amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    deductible = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policies_effective_date",
                table: "policies",
                column: "effective_date");

            migrationBuilder.CreateIndex(
                name: "IX_policies_policy_number",
                table: "policies",
                column: "policy_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policies_type_status",
                table: "policies",
                columns: new[] { "type", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policies");
        }
    }
}
