using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TrainingPlanOnDeleteCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanEntityId",
                table: "TrainingPlanBlock");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanEntityId",
                table: "TrainingPlanBlock",
                column: "TrainingPlanEntityId",
                principalTable: "TrainingPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry",
                column: "TrainingPlanBlockEntityId",
                principalTable: "TrainingPlanBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanEntityId",
                table: "TrainingPlanBlock");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanEntityId",
                table: "TrainingPlanBlock",
                column: "TrainingPlanEntityId",
                principalTable: "TrainingPlan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry",
                column: "TrainingPlanBlockEntityId",
                principalTable: "TrainingPlanBlock",
                principalColumn: "Id");
        }
    }
}
