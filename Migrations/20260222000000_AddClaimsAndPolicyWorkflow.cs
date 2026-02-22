using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimsAndPolicyWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to InsurancePolicies
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationDescription",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "InsurancePolicies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "InsurancePolicies",
                type: "TEXT",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByAdminId",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "InsurancePolicies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminRemarks",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            // Create Claims table
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InsurancePolicyId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "Submitted"),
                    ReviewedByAdminId = table.Column<int>(type: "INTEGER", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AdminRemarks = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SettledAmount = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_InsurancePolicies_InsurancePolicyId",
                        column: x => x.InsurancePolicyId,
                        principalTable: "InsurancePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Claims_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_InsurancePolicyId",
                table: "Claims",
                column: "InsurancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Claims");

            migrationBuilder.DropColumn(name: "UserId", table: "InsurancePolicies");
            migrationBuilder.DropColumn(name: "ApplicationDescription", table: "InsurancePolicies");
            migrationBuilder.DropColumn(name: "ApplicationDate", table: "InsurancePolicies");
            migrationBuilder.DropColumn(name: "Status", table: "InsurancePolicies");
            migrationBuilder.DropColumn(name: "ReviewedByAdminId", table: "InsurancePolicies");
            migrationBuilder.DropColumn(name: "ReviewedAt", table: "InsurancePolicies");
            migrationBuilder.DropColumn(name: "AdminRemarks", table: "InsurancePolicies");
        }
    }
}
