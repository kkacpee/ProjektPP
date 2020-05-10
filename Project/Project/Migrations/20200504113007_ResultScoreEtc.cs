using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Project.Migrations
{
    public partial class ResultScoreEtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Search",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phrase = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Search", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Histories_SearchId",
                table: "Histories",
                column: "SearchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Search_SearchId",
                table: "Histories",
                column: "SearchId",
                principalTable: "Search",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Search_SearchId",
                table: "Histories");

            migrationBuilder.DropTable(
                name: "Search");

            migrationBuilder.DropIndex(
                name: "IX_Histories_SearchId",
                table: "Histories");
        }
    }
}
