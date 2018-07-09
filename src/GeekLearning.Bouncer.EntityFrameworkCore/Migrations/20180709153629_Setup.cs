using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GeekLearning.Bouncer.EntityFrameworkCore.Migrations
{
    public partial class Setup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModelModificationDate",
                columns: table => new
                {
                    Id = table.Column<byte>(nullable: false),
                    Rights = table.Column<DateTime>(nullable: false),
                    Roles = table.Column<DateTime>(nullable: false),
                    Scopes = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelModificationDate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Principal",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Principal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Right",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Right", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scope",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Group_Principal_Id",
                        column: x => x.Id,
                        principalTable: "Principal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleRight",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(nullable: false),
                    RightId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleRight", x => new { x.RoleId, x.RightId });
                    table.ForeignKey(
                        name: "FK_RoleRight_Right_RightId",
                        column: x => x.RightId,
                        principalTable: "Right",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleRight_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Authorization",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false),
                    ScopeId = table.Column<Guid>(nullable: false),
                    PrincipalId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Authorization_Principal_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Principal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Authorization_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Authorization_Scope_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "Scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScopeHierarchy",
                columns: table => new
                {
                    ParentId = table.Column<Guid>(nullable: false),
                    ChildId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScopeHierarchy", x => new { x.ParentId, x.ChildId });
                    table.ForeignKey(
                        name: "FK_ScopeHierarchy_Scope_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScopeHierarchy_Scope_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Scope",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Membership",
                columns: table => new
                {
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ModificationBy = table.Column<Guid>(nullable: false),
                    ModificationDate = table.Column<DateTime>(nullable: false),
                    CreationBy = table.Column<Guid>(nullable: false),
                    PrincipalId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false),
                    IsDeletable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membership", x => new { x.GroupId, x.PrincipalId });
                    table.ForeignKey(
                        name: "FK_Membership_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Membership_Principal_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Principal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authorization_PrincipalId",
                table: "Authorization",
                column: "PrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Authorization_ScopeId",
                table: "Authorization",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_Authorization_RoleId_ScopeId_PrincipalId",
                table: "Authorization",
                columns: new[] { "RoleId", "ScopeId", "PrincipalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Group_Name",
                table: "Group",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membership_PrincipalId",
                table: "Membership",
                column: "PrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Right_Name",
                table: "Right",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Name",
                table: "Role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleRight_RightId",
                table: "RoleRight",
                column: "RightId");

            migrationBuilder.CreateIndex(
                name: "IX_Scope_Name",
                table: "Scope",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScopeHierarchy_ChildId",
                table: "ScopeHierarchy",
                column: "ChildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Authorization");

            migrationBuilder.DropTable(
                name: "Membership");

            migrationBuilder.DropTable(
                name: "ModelModificationDate");

            migrationBuilder.DropTable(
                name: "RoleRight");

            migrationBuilder.DropTable(
                name: "ScopeHierarchy");

            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropTable(
                name: "Right");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Scope");

            migrationBuilder.DropTable(
                name: "Principal");
        }
    }
}
