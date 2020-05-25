using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Amver.EfCli.Migrations
{
    public partial class UserToBlockedUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserToBlockedUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    BlockedUserId = table.Column<int>(nullable: false),
                    BlockingDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToBlockedUsers", x => new { x.UserId, x.BlockedUserId });
                    table.ForeignKey(
                        name: "FK_UserToBlockedUsers_Users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToBlockedUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserToBlockedUsers_BlockedUserId",
                table: "UserToBlockedUsers",
                column: "BlockedUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserToBlockedUsers");
        }
    }
}
