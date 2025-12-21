using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FZ.WebAPI.Migrations.MovieDb
{
    /// <inheritdoc />
    public partial class MovieV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rawSubTitle",
                schema: "movie",
                table: "EpisodeSource",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EpisodeSubTitles",
                columns: table => new
                {
                    episodeSubTitleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    episodeSourceID = table.Column<int>(type: "integer", nullable: false),
                    subTitleName = table.Column<string>(type: "text", nullable: false),
                    linkSubTitle = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeSubTitles", x => x.episodeSubTitleID);
                    table.ForeignKey(
                        name: "FK_EpisodeSubTitles_EpisodeSource_episodeSourceID",
                        column: x => x.episodeSourceID,
                        principalSchema: "movie",
                        principalTable: "EpisodeSource",
                        principalColumn: "episodeSourceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeSubTitles_episodeSourceID",
                table: "EpisodeSubTitles",
                column: "episodeSourceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EpisodeSubTitles");

            migrationBuilder.DropColumn(
                name: "rawSubTitle",
                schema: "movie",
                table: "EpisodeSource");
        }
    }
}
