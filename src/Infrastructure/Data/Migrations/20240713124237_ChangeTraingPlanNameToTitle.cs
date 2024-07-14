using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTraingPlanNameToTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TrainingPlanBlock",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TrainingPlan",
                newName: "Title");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPlan_Name",
                table: "TrainingPlan",
                newName: "IX_TrainingPlan_Title");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPlan_AuthorId_Name",
                table: "TrainingPlan",
                newName: "IX_TrainingPlan_AuthorId_Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TrainingPlanBlock",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "TrainingPlan",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPlan_Title",
                table: "TrainingPlan",
                newName: "IX_TrainingPlan_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingPlan_AuthorId_Title",
                table: "TrainingPlan",
                newName: "IX_TrainingPlan_AuthorId_Name");
        }
    }
}
