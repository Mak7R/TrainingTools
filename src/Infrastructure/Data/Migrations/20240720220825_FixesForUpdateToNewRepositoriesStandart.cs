using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixesForUpdateToNewRepositoriesStandart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendInvitation_AspNetUsers_TargetId",
                table: "FriendInvitation");

            migrationBuilder.DropTable(
                name: "FriendRelationship");

            migrationBuilder.RenameColumn(
                name: "Desctiption",
                table: "TrainingPlanBlockEntry",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "InvitationTime",
                table: "FriendInvitation",
                newName: "InvitationDateTime");

            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "FriendInvitation",
                newName: "InvitedId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendInvitation_TargetId",
                table: "FriendInvitation",
                newName: "IX_FriendInvitation_InvitedId");

            migrationBuilder.CreateTable(
                name: "Friendship",
                columns: table => new
                {
                    FirstFriendId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecondFriendId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FriendsFrom = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendship", x => new { x.FirstFriendId, x.SecondFriendId });
                    table.ForeignKey(
                        name: "FK_Friendship_AspNetUsers_FirstFriendId",
                        column: x => x.FirstFriendId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Friendship_AspNetUsers_SecondFriendId",
                        column: x => x.SecondFriendId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friendship_SecondFriendId",
                table: "Friendship",
                column: "SecondFriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendInvitation_AspNetUsers_InvitedId",
                table: "FriendInvitation",
                column: "InvitedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendInvitation_AspNetUsers_InvitedId",
                table: "FriendInvitation");

            migrationBuilder.DropTable(
                name: "Friendship");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TrainingPlanBlockEntry",
                newName: "Desctiption");

            migrationBuilder.RenameColumn(
                name: "InvitationDateTime",
                table: "FriendInvitation",
                newName: "InvitationTime");

            migrationBuilder.RenameColumn(
                name: "InvitedId",
                table: "FriendInvitation",
                newName: "TargetId");

            migrationBuilder.RenameIndex(
                name: "IX_FriendInvitation_InvitedId",
                table: "FriendInvitation",
                newName: "IX_FriendInvitation_TargetId");

            migrationBuilder.CreateTable(
                name: "FriendRelationship",
                columns: table => new
                {
                    FirstFriendId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecondFriendId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FriendsFrom = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRelationship", x => new { x.FirstFriendId, x.SecondFriendId });
                    table.ForeignKey(
                        name: "FK_FriendRelationship_AspNetUsers_FirstFriendId",
                        column: x => x.FirstFriendId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FriendRelationship_AspNetUsers_SecondFriendId",
                        column: x => x.SecondFriendId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRelationship_SecondFriendId",
                table: "FriendRelationship",
                column: "SecondFriendId");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendInvitation_AspNetUsers_TargetId",
                table: "FriendInvitation",
                column: "TargetId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
