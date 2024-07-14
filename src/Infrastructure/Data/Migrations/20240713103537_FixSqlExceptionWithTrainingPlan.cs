using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSqlExceptionWithTrainingPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanId",
                table: "TrainingPlanBlock");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingPlanBlockEntry",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlanBlockEntry_TrainingPlanBlockId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlanBlock_TrainingPlanId",
                table: "TrainingPlanBlock");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlanBlock_TrainingPlanId_Position",
                table: "TrainingPlanBlock");

            migrationBuilder.DropColumn(
                name: "TrainingPlanId",
                table: "TrainingPlanBlock");

            migrationBuilder.RenameColumn(
                name: "TrainingPlanBlockId",
                table: "TrainingPlanBlockEntry",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingPlanEntityId",
                table: "TrainingPlanBlock",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingPlanBlockEntry",
                table: "TrainingPlanBlockEntry",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanBlockEntry_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry",
                column: "TrainingPlanBlockEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanBlock_TrainingPlanEntityId",
                table: "TrainingPlanBlock",
                column: "TrainingPlanEntityId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanEntityId",
                table: "TrainingPlanBlock");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TrainingPlanBlockEntry",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlanBlockEntry_TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropIndex(
                name: "IX_TrainingPlanBlock_TrainingPlanEntityId",
                table: "TrainingPlanBlock");

            migrationBuilder.DropColumn(
                name: "TrainingPlanBlockEntityId",
                table: "TrainingPlanBlockEntry");

            migrationBuilder.DropColumn(
                name: "TrainingPlanEntityId",
                table: "TrainingPlanBlock");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "TrainingPlanBlockEntry",
                newName: "TrainingPlanBlockId");

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingPlanId",
                table: "TrainingPlanBlock",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrainingPlanBlockEntry",
                table: "TrainingPlanBlockEntry",
                columns: new[] { "TrainingPlanBlockId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanBlockEntry_TrainingPlanBlockId",
                table: "TrainingPlanBlockEntry",
                column: "TrainingPlanBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanBlock_TrainingPlanId",
                table: "TrainingPlanBlock",
                column: "TrainingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlanBlock_TrainingPlanId_Position",
                table: "TrainingPlanBlock",
                columns: new[] { "TrainingPlanId", "Position" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlanBlock_TrainingPlan_TrainingPlanId",
                table: "TrainingPlanBlock",
                column: "TrainingPlanId",
                principalTable: "TrainingPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingPlanBlockEntry_TrainingPlanBlock_TrainingPlanBlockId",
                table: "TrainingPlanBlockEntry",
                column: "TrainingPlanBlockId",
                principalTable: "TrainingPlanBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
