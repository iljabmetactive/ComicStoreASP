using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComicStoreASP.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchAnalyticsLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchAnalyticsLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SavedSearchId = table.Column<int>(type: "int", nullable: false),
                    ComicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchAnalyticsLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchAnalyticsLogs_DataComics_ComicId",
                        column: x => x.ComicId,
                        principalTable: "DataComics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchAnalyticsLogs_SavedSearches_SavedSearchId",
                        column: x => x.SavedSearchId,
                        principalTable: "SavedSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchAnalyticsLogs_ComicId",
                table: "SearchAnalyticsLogs",
                column: "ComicId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchAnalyticsLogs_SavedSearchId",
                table: "SearchAnalyticsLogs",
                column: "SavedSearchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchAnalyticsLogs");
        }
    }
}
