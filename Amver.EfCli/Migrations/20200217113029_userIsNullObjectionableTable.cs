using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Amver.EfCli.Migrations
{
    public partial class userIsNullObjectionableTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectionableContents_Users_UserId",
                table: "ObjectionableContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectionableContents",
                table: "ObjectionableContents");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ObjectionableContents",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ObjectionableContents",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectionableContents",
                table: "ObjectionableContents",
                column: "Id");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectionableContents_Users_UserId",
                table: "ObjectionableContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObjectionableContents",
                table: "ObjectionableContents");

            migrationBuilder.DropIndex(
                name: "IX_ObjectionableContents_UserId",
                table: "ObjectionableContents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ObjectionableContents");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ObjectionableContents",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObjectionableContents",
                table: "ObjectionableContents",
                columns: new[] { "UserId", "ObjectionableUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectionableContents_Users_UserId",
                table: "ObjectionableContents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
