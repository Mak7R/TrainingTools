using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingTools.Migrations
{
    /// <inheritdoc />
    public partial class GroupsAddedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Exercises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_GroupId",
                table: "Exercises",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_WorkspaceId",
                table: "Groups",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Groups_GroupId",
                table: "Exercises",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Groups_GroupId",
                table: "Exercises");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_GroupId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Exercises");
        }
    }
}
