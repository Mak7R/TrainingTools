using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedAboutToExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Group_Name",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_Name_GroupId",
                table: "Exercise");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Group",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercise",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Exercise",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Group_Name",
                table: "Group",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name_GroupId",
                table: "Exercise",
                columns: new[] { "Name", "GroupId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Group_Name",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Exercise_Name_GroupId",
                table: "Exercise");

            migrationBuilder.DropColumn(
                name: "About",
                table: "Exercise");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Group",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Exercise",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "IX_Group_Name",
                table: "Group",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name_GroupId",
                table: "Exercise",
                columns: new[] { "Name", "GroupId" },
                unique: true,
                filter: "[Name] IS NOT NULL");
        }
    }
}
