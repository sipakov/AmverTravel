using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Amver.EfCli.Migrations
{
    public partial class AddObjectionableReasonTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjectionableReasons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Reason = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectionableReasons", x => x.Id);
                });

            
            migrationBuilder.CreateTable(
                name: "ObjectionableContents",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    ObjectionableUserId = table.Column<int>(nullable: false),
                    ObjectionableReasonId = table.Column<int>(nullable: false),
                    TripId = table.Column<int>(nullable: true),
                    Comment = table.Column<string>(maxLength: 500, nullable: true),
                    BanDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectionableContents", x => new { x.UserId, x.ObjectionableUserId });
                    table.ForeignKey(
                        name: "FK_ObjectionableContents_ObjectionableReasons_ObjectionableRea~",
                        column: x => x.ObjectionableReasonId,
                        principalTable: "ObjectionableReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectionableContents_Users_ObjectionableUserId",
                        column: x => x.ObjectionableUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObjectionableContents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "Trips",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectionableContents_ObjectionableReasonId",
                table: "ObjectionableContents",
                column: "ObjectionableReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectionableContents_ObjectionableUserId",
                table: "ObjectionableContents",
                column: "ObjectionableUserId");

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectionableContents");
            

            migrationBuilder.DropTable(
                name: "ObjectionableReasons");
            
            migrationBuilder.DropColumn(
                name: "Trips",
                table: "IsBanned");
        }
    }
}
