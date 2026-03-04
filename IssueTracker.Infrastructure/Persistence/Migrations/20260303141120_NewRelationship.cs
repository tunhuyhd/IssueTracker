using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRoles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<string>(type: "text", nullable: false),
                    OwnerId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_users_OwnerId1",
                        column: x => x.OwnerId1,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_ProjectPermissionProjectRole_ProjectRoles_ProjectRolesId",
                        column: x => x.ProjectRolesId,
                        principalTable: "ProjectRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectPermissionProjectRole_project_permissions_ProjectPer~",
                        column: x => x.ProjectPermissionsId,
                        principalTable: "project_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_modified_by = table.Column<Guid>(type: "uuid", nullable: false),
                    last_modified_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_on = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_projects_ProjectRoles_project_role_id",
                        column: x => x.project_role_id,
                        principalTable: "ProjectRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_projects_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_projects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPermissionProjectRole_ProjectRolesId",
                table: "ProjectPermissionProjectRole",
                column: "ProjectRolesId");

            migrationBuilder.CreateIndex(
                name: "IX_projects_OwnerId1",
                table: "projects",
                column: "OwnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_user_projects_project_id",
                table: "user_projects",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_projects_project_role_id",
                table: "user_projects",
                column: "project_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_projects_user_id",
                table: "user_projects",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectPermissionProjectRole");

            migrationBuilder.DropTable(
                name: "user_projects");

            migrationBuilder.DropTable(
                name: "project_permissions");

            migrationBuilder.DropTable(
                name: "ProjectRoles");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
