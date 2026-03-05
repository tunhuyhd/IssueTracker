using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectOwnerIdTypeWithConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_OwnerId1",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_OwnerId1",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "projects");

            // Custom SQL to convert string to uuid using USING clause
            migrationBuilder.Sql(@"
                ALTER TABLE projects 
                ALTER COLUMN owner_id TYPE uuid 
                USING owner_id::uuid;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_projects_owner_id",
                table: "projects",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_owner_id",
                table: "projects",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_projects_users_owner_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "IX_projects_owner_id",
                table: "projects");

            migrationBuilder.AlterColumn<string>(
                name: "owner_id",
                table: "projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId1",
                table: "projects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_projects_OwnerId1",
                table: "projects",
                column: "OwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_projects_users_OwnerId1",
                table: "projects",
                column: "OwnerId1",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
