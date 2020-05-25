using Microsoft.EntityFrameworkCore.Migrations;

namespace Amver.EfCli.Migrations
{
    public partial class AddLocalizationRuColumnsCityCountryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ruRu",
                table: "Countries",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ruRu",
                table: "Cities",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ruRu",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "ruRu",
                table: "Cities");
        }
    }
}
