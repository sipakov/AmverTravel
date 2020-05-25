using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Amver.EfCli.Migrations
{
    public partial class UserToFcmToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserToFcmTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    FcmToken = table.Column<string>(maxLength: 500, nullable: false),
                    LastSignIn = table.Column<DateTime>(nullable: false),
                    LastVisit = table.Column<DateTime>(nullable: false),
                    IsInApp = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToFcmTokens", x => new { x.UserId, x.FcmToken });
                    table.ForeignKey(
                        name: "FK_UserToFcmTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserToFcmTokens");
        }
    }
}
