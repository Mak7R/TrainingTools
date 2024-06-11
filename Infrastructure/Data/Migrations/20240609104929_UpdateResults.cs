using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerializedResults",
                table: "ExerciseResult");

            migrationBuilder.AddColumn<string>(
                name: "Results",
                table: "ExerciseResult",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Group_Name",
                table: "Group",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name",
                table: "Exercise",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Group_Name",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_Name",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "Results",
                table: "ExerciseResult");

            migrationBuilder.AddColumn<string>(
                name: "SerializedResults",
                table: "ExerciseResult",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }
    }
}
