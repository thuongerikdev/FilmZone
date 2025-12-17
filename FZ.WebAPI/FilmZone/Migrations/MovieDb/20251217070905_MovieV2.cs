using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FZ.WebAPI.Migrations.MovieDb
{
    /// <inheritdoc />
    public partial class MovieV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rawSubTitle",
                schema: "movie",
                table: "MovieSource",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MovieSubTitle",
                schema: "movie",
                columns: table => new
                {
                    movieSubTitleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieSourceID = table.Column<int>(type: "integer", nullable: false),
                    subTitleName = table.Column<string>(type: "text", nullable: false),
                    linkSubTitle = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieSubTitle", x => x.movieSubTitleID);
                    table.ForeignKey(
                        name: "FK_MovieSubTitle_MovieSource_movieSourceID",
                        column: x => x.movieSourceID,
                        principalSchema: "movie",
                        principalTable: "MovieSource",
                        principalColumn: "movieSourceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovieSubTitle_movieSourceID",
                schema: "movie",
                table: "MovieSubTitle",
                column: "movieSourceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovieSubTitle",
                schema: "movie");

            migrationBuilder.DropColumn(
                name: "rawSubTitle",
                schema: "movie",
                table: "MovieSource");
        }
    }
}
