using Microsoft.EntityFrameworkCore.Migrations;

namespace Project.Migrations
{
    public partial class ResultScoreDbSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Search_SearchId",
                table: "Histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Search",
                table: "Search");

            migrationBuilder.RenameTable(
                name: "Search",
                newName: "Searches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Searches",
                table: "Searches",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SearchId = table.Column<int>(nullable: false),
                    PhotoId = table.Column<int>(nullable: false),
                    EngineId = table.Column<int>(nullable: false),
                    AvgScore = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Results_Engines_EngineId",
                        column: x => x.EngineId,
                        principalTable: "Engines",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_Searches_SearchId",
                        column: x => x.SearchId,
                        principalTable: "Searches",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grade = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    ResultId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Scores_Results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Results",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scores_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Results_EngineId",
                table: "Results",
                column: "EngineId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_PhotoId",
                table: "Results",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_SearchId",
                table: "Results",
                column: "SearchId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_ResultId",
                table: "Scores",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_UserId",
                table: "Scores",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Searches_SearchId",
                table: "Histories",
                column: "SearchId",
                principalTable: "Searches",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Searches_SearchId",
                table: "Histories");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Searches",
                table: "Searches");

            migrationBuilder.RenameTable(
                name: "Searches",
                newName: "Search");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Search",
                table: "Search",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Search_SearchId",
                table: "Histories",
                column: "SearchId",
                principalTable: "Search",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
