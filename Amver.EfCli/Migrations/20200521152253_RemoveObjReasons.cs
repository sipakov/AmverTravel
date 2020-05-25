using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Amver.EfCli.Migrations
{
    public partial class RemoveObjReasons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectionableContents_ObjectionableReasons_ObjectionableRea~",
                table: "ObjectionableContents");

            migrationBuilder.DropTable(
                name: "ObjectionableReasons");

            migrationBuilder.DropTable(
                name: "UserToInstalls");

            migrationBuilder.DropIndex(
                name: "IX_ObjectionableContents_ObjectionableReasonId",
                table: "ObjectionableContents");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjectionableReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectionableReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserToInstalls",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    InstallId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsInApp = table.Column<bool>(type: "boolean", nullable: false),
                    LastSignIn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ObjectionableContents_ObjectionableReasonId",
                table: "ObjectionableContents",
                column: "ObjectionableReasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectionableContents_ObjectionableReasons_ObjectionableRea~",
                table: "ObjectionableContents",
                column: "ObjectionableReasonId",
                principalTable: "ObjectionableReasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
