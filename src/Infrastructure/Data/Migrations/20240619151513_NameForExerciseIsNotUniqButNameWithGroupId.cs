using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NameForExerciseIsNotUniqButNameWithGroupId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exercise_Name",
                table: "Exercise");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name",
                table: "Exercise",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name_GroupId",
                table: "Exercise",
                columns: new[] { "Name", "GroupId" },
                unique: true,
                filter: "[Name] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exercise_Name",
                table: "Exercise");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_Name_GroupId",
                table: "Exercise");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name",
                table: "Exercise",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");
        }
    }
}
