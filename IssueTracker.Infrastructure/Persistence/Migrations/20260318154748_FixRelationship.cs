using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectPermissionProjectRole");

            migrationBuilder.CreateTable(
                name: "project_role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_role_permissions_project_permissions_project_permis~",
                        column: x => x.project_permission_id,
                        principalTable: "project_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_role_permissions_project_roles_project_role_id",
                        column: x => x.project_role_id,
                        principalTable: "project_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_role_permissions_project_permission_id",
                table: "project_role_permissions",
                column: "project_permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_role_permissions_project_role_id",
                table: "project_role_permissions",
                column: "project_role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_role_permissions");

            migrationBuilder.CreateTable(
                name: "ProjectPermissionProjectRole",
                columns: table => new
                {
                    ProjectPermissionsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectRolesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPermissionProjectRole", x => new { x.ProjectPermissionsId, x.ProjectRolesId });
                    table.ForeignKey(
                        name: "FK_ProjectPermissionProjectRole_project_permissions_ProjectPer~",
                        column: x => x.ProjectPermissionsId,
                        principalTable: "project_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectPermissionProjectRole_project_roles_ProjectRolesId",
                        column: x => x.ProjectRolesId,
                        principalTable: "project_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPermissionProjectRole_ProjectRolesId",
                table: "ProjectPermissionProjectRole",
                column: "ProjectRolesId");
        }
    }
}
