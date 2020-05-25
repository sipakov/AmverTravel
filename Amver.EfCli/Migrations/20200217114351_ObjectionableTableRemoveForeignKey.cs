using Microsoft.EntityFrameworkCore.Migrations;

namespace Amver.EfCli.Migrations
{
    public partial class ObjectionableTableRemoveForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectionableContents_Users_UserId",
                table: "ObjectionableContents");

            migrationBuilder.DropIndex(
                name: "IX_ObjectionableContents_UserId",
                table: "ObjectionableContents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ObjectionableContents_UserId",
                table: "ObjectionableContents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectionableContents_Users_UserId",
                table: "ObjectionableContents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
