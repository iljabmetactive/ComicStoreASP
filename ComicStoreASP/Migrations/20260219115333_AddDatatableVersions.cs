using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComicStoreASP.Migrations
{
    /// <inheritdoc />
    public partial class AddDatatableVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DatasetVersionId",
                table: "DataComics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DatatableVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatatableVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataComics_DatasetVersionId",
                table: "DataComics",
                column: "DatasetVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataComics_DatatableVersions_DatasetVersionId",
                table: "DataComics",
                column: "DatasetVersionId",
                principalTable: "DatatableVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataComics_DatatableVersions_DatasetVersionId",
                table: "DataComics");

            migrationBuilder.DropTable(
                name: "DatatableVersions");

            migrationBuilder.DropIndex(
                name: "IX_DataComics_DatasetVersionId",
                table: "DataComics");

            migrationBuilder.DropColumn(
                name: "DatasetVersionId",
                table: "DataComics");
        }
    }
}
