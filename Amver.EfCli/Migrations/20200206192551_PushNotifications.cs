using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Amver.EfCli.Migrations
{
    public partial class PushNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserToInstalls",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    InstallId = table.Column<Guid>(nullable: false),
                    LastSignIn = table.Column<DateTime>(nullable: false),
                    IsInApp = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToInstalls", x => new { x.UserId, x.InstallId });
                    table.ForeignKey(
                        name: "FK_UserToInstalls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserToInstalls");
        }
    }
}
