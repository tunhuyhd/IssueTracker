using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixNameOftable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPermissionProjectRole_ProjectRoles_ProjectRolesId",
                table: "ProjectPermissionProjectRole");

            migrationBuilder.DropForeignKey(
                name: "FK_user_projects_ProjectRoles_project_role_id",
                table: "user_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_projects",
                table: "user_projects");

            migrationBuilder.DropIndex(
                name: "IX_user_projects_user_id",
                table: "user_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectRoles",
                table: "ProjectRoles");

            migrationBuilder.RenameTable(
                name: "ProjectRoles",
                newName: "project_roles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_projects",
                table: "user_projects",
                columns: new[] { "user_id", "project_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_project_roles",
                table: "project_roles",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPermissionProjectRole_project_roles_ProjectRolesId",
                table: "ProjectPermissionProjectRole",
                column: "ProjectRolesId",
                principalTable: "project_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_projects_project_roles_project_role_id",
                table: "user_projects",
                column: "project_role_id",
                principalTable: "project_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPermissionProjectRole_project_roles_ProjectRolesId",
                table: "ProjectPermissionProjectRole");

            migrationBuilder.DropForeignKey(
                name: "FK_user_projects_project_roles_project_role_id",
                table: "user_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_projects",
                table: "user_projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_project_roles",
                table: "project_roles");

            migrationBuilder.RenameTable(
                name: "project_roles",
                newName: "ProjectRoles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_projects",
                table: "user_projects",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectRoles",
                table: "ProjectRoles",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_user_projects_user_id",
                table: "user_projects",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPermissionProjectRole_ProjectRoles_ProjectRolesId",
                table: "ProjectPermissionProjectRole",
                column: "ProjectRolesId",
                principalTable: "ProjectRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_projects_ProjectRoles_project_role_id",
                table: "user_projects",
                column: "project_role_id",
                principalTable: "ProjectRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
