using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingTools.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceFollowersAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Workspaces",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FollowerRelationships",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FollowerRights = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerRelationships", x => new { x.WorkspaceId, x.FollowerId });
                    table.ForeignKey(
                        name: "FK_FollowerRelationships_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FollowerRelationships_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelationships_FollowerId",
                table: "FollowerRelationships",
                column: "FollowerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowerRelationships");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Workspaces");
        }
    }
}
