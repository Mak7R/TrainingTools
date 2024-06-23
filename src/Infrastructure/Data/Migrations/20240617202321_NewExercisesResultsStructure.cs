using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewExercisesResultsStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseResult_AspNetUsers_UserId",
                table: "ExerciseResult");

            migrationBuilder.RenameColumn(
                name: "Results",
                table: "ExerciseResult",
                newName: "Comments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ExerciseResult",
                newName: "OwnerId");

            migrationBuilder.AddColumn<string>(
                name: "Counts",
                table: "ExerciseResult",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Weights",
                table: "ExerciseResult",
                type: "nvarchar(144)",
                maxLength: 144,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseResult_AspNetUsers_OwnerId",
                table: "ExerciseResult",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseResult_AspNetUsers_OwnerId",
                table: "ExerciseResult");

            migrationBuilder.DropColumn(
                name: "Counts",
                table: "ExerciseResult");

            migrationBuilder.DropColumn(
                name: "Weights",
                table: "ExerciseResult");

            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "ExerciseResult",
                newName: "Results");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "ExerciseResult",
                newName: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseResult_AspNetUsers_UserId",
                table: "ExerciseResult",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
