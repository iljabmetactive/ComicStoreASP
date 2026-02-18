using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComicStoreASP.Migrations
{
    /// <inheritdoc />
    public partial class AddComicFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SavedComics_ComicId",
                table: "SavedComics",
                column: "ComicId");

            migrationBuilder.CreateIndex(
                name: "IX_ComicFlags_ComicId",
                table: "ComicFlags",
                column: "ComicId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComicFlags_DataComics_ComicId",
                table: "ComicFlags",
                column: "ComicId",
                principalTable: "DataComics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedComics_DataComics_ComicId",
                table: "SavedComics",
                column: "ComicId",
                principalTable: "DataComics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComicFlags_DataComics_ComicId",
                table: "ComicFlags");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedComics_DataComics_ComicId",
                table: "SavedComics");

            migrationBuilder.DropIndex(
                name: "IX_SavedComics_ComicId",
                table: "SavedComics");

            migrationBuilder.DropIndex(
                name: "IX_ComicFlags_ComicId",
                table: "ComicFlags");
        }
    }
}
