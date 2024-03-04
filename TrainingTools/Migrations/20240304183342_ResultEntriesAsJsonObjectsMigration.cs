using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingTools.Migrations
{
    /// <inheritdoc />
    public partial class ResultEntriesAsJsonObjectsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseResultEntries");

            migrationBuilder.AddColumn<string>(
                name: "ResultsJson",
                table: "ExerciseResults",
                type: "nvarchar(max)",
                maxLength: 128000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultsJson",
                table: "ExerciseResults");

            migrationBuilder.CreateTable(
                name: "ExerciseResultEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    ExerciseResultsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseResultEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseResultEntries_ExerciseResults_ExerciseResultsId",
                        column: x => x.ExerciseResultsId,
                        principalTable: "ExerciseResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseResultEntries_ExerciseResultsId",
                table: "ExerciseResultEntries",
                column: "ExerciseResultsId");
        }
    }
}
