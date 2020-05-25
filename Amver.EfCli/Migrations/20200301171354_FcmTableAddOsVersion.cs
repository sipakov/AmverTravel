using Microsoft.EntityFrameworkCore.Migrations;

namespace Amver.EfCli.Migrations
{
    public partial class FcmTableAddOsVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Os",
                table: "UserToFcmTokens",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Os",
                table: "UserToFcmTokens");
        }
    }
}
