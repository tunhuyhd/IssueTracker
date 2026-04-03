using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_login_provider",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_login_provider_key",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_login_provider",
                table: "users");

            migrationBuilder.DropColumn(
                name: "external_login_provider_key",
                table: "users");
        }
    }
}
