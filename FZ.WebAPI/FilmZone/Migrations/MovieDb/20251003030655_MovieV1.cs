using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FZ.WebAPI.Migrations.MovieDb
{
    /// <inheritdoc />
    public partial class MovieV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "movie");

            migrationBuilder.CreateTable(
                name: "ImageSource",
                schema: "movie",
                columns: table => new
                {
                    imageSourceID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imageSourceName = table.Column<string>(type: "text", nullable: false),
                    imageSourcetype = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageSource", x => x.imageSourceID);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                schema: "movie",
                columns: table => new
                {
                    regionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.regionID);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "movie",
                columns: table => new
                {
                    tagID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tagName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tagDescription = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    createAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.tagID);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                schema: "movie",
                columns: table => new
                {
                    movieID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    originalTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    movieType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    releaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    durationSeconds = table.Column<int>(type: "integer", nullable: true),
                    totalSeasons = table.Column<int>(type: "integer", nullable: true),
                    totalEpisodes = table.Column<int>(type: "integer", nullable: true),
                    regionID = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: true),
                    rated = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    popularity = table.Column<double>(type: "double precision", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.movieID);
                    table.ForeignKey(
                        name: "FK_Movies_Region_regionID",
                        column: x => x.regionID,
                        principalSchema: "movie",
                        principalTable: "Region",
                        principalColumn: "regionID");
                });

            migrationBuilder.CreateTable(
                name: "Person",
                schema: "movie",
                columns: table => new
                {
                    personID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    knownFor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    regionID = table.Column<int>(type: "integer", nullable: false),
                    biography = table.Column<string>(type: "text", nullable: true),
                    avatar = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    birthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.personID);
                    table.ForeignKey(
                        name: "FK_Person_Region_regionID",
                        column: x => x.regionID,
                        principalSchema: "movie",
                        principalTable: "Region",
                        principalColumn: "regionID");
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                schema: "movie",
                columns: table => new
                {
                    commentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    userID = table.Column<int>(type: "integer", nullable: false),
                    parentID = table.Column<int>(type: "integer", nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    isEdited = table.Column<bool>(type: "boolean", nullable: false),
                    likeCount = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.commentID);
                    table.ForeignKey(
                        name: "FK_Comment_Comment_parentID",
                        column: x => x.parentID,
                        principalSchema: "movie",
                        principalTable: "Comment",
                        principalColumn: "commentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comment_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Episode",
                schema: "movie",
                columns: table => new
                {
                    episodeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    seasonNumber = table.Column<int>(type: "integer", nullable: false),
                    episodeNumber = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    synopsis = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    durationSeconds = table.Column<int>(type: "integer", nullable: true),
                    releaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episode", x => x.episodeID);
                    table.ForeignKey(
                        name: "FK_Episode_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieImage",
                schema: "movie",
                columns: table => new
                {
                    movieImageID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieImage", x => x.movieImageID);
                    table.ForeignKey(
                        name: "FK_MovieImage_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieSource",
                schema: "movie",
                columns: table => new
                {
                    movieSourceID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    sourceName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    sourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    sourceUrl = table.Column<string>(type: "text", nullable: false),
                    sourceID = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    quality = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    isVipOnly = table.Column<bool>(type: "boolean", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieSource", x => x.movieSourceID);
                    table.ForeignKey(
                        name: "FK_MovieSource_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieTag",
                schema: "movie",
                columns: table => new
                {
                    movieTagID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    tagID = table.Column<int>(type: "integer", nullable: false),
                    createAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieTag", x => x.movieTagID);
                    table.ForeignKey(
                        name: "FK_MovieTag_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieTag_Tag_tagID",
                        column: x => x.tagID,
                        principalSchema: "movie",
                        principalTable: "Tag",
                        principalColumn: "tagID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedMovie",
                schema: "movie",
                columns: table => new
                {
                    savedMovieID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    userID = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedMovie", x => x.savedMovieID);
                    table.ForeignKey(
                        name: "FK_SavedMovie_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRating",
                schema: "movie",
                columns: table => new
                {
                    userID = table.Column<int>(type: "integer", nullable: false),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    userRatingID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRating", x => new { x.userID, x.movieID });
                    table.ForeignKey(
                        name: "FK_UserRating_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoviePerson",
                schema: "movie",
                columns: table => new
                {
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    personID = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    moviePersonID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    characterName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    creditOrder = table.Column<int>(type: "integer", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviePerson", x => new { x.movieID, x.personID, x.role });
                    table.ForeignKey(
                        name: "FK_MoviePerson_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoviePerson_Person_personID",
                        column: x => x.personID,
                        principalSchema: "movie",
                        principalTable: "Person",
                        principalColumn: "personID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EpisodeSource",
                schema: "movie",
                columns: table => new
                {
                    episodeSourceID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    episodeID = table.Column<int>(type: "integer", nullable: false),
                    sourceName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    sourceType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    sourceUrl = table.Column<string>(type: "text", nullable: false),
                    sourceID = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    quality = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    language = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    isVipOnly = table.Column<bool>(type: "boolean", nullable: false),
                    isActive = table.Column<bool>(type: "boolean", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeSource", x => x.episodeSourceID);
                    table.ForeignKey(
                        name: "FK_EpisodeSource_Episode_episodeID",
                        column: x => x.episodeID,
                        principalSchema: "movie",
                        principalTable: "Episode",
                        principalColumn: "episodeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchProgress",
                schema: "movie",
                columns: table => new
                {
                    watchProgressID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userID = table.Column<int>(type: "integer", nullable: false),
                    movieID = table.Column<int>(type: "integer", nullable: false),
                    sourceID = table.Column<int>(type: "integer", nullable: true),
                    positionSeconds = table.Column<int>(type: "integer", nullable: false),
                    durationSeconds = table.Column<int>(type: "integer", nullable: true),
                    lastWatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MoviesmovieID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchProgress", x => x.watchProgressID);
                    table.ForeignKey(
                        name: "FK_WatchProgress_MovieSource_sourceID",
                        column: x => x.sourceID,
                        principalSchema: "movie",
                        principalTable: "MovieSource",
                        principalColumn: "movieSourceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchProgress_Movies_MoviesmovieID",
                        column: x => x.MoviesmovieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID");
                    table.ForeignKey(
                        name: "FK_WatchProgress_Movies_movieID",
                        column: x => x.movieID,
                        principalSchema: "movie",
                        principalTable: "Movies",
                        principalColumn: "movieID");
                });

            migrationBuilder.CreateTable(
                name: "EpisodeWatchProgress",
                schema: "movie",
                columns: table => new
                {
                    episodeWatchProgressID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userID = table.Column<int>(type: "integer", nullable: false),
                    episodeID = table.Column<int>(type: "integer", nullable: false),
                    episodeSourceID = table.Column<int>(type: "integer", nullable: true),
                    positionSeconds = table.Column<int>(type: "integer", nullable: false),
                    durationSeconds = table.Column<int>(type: "integer", nullable: true),
                    lastWatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EpisodeWatchProgress", x => x.episodeWatchProgressID);
                    table.ForeignKey(
                        name: "FK_EpisodeWatchProgress_EpisodeSource_episodeSourceID",
                        column: x => x.episodeSourceID,
                        principalSchema: "movie",
                        principalTable: "EpisodeSource",
                        principalColumn: "episodeSourceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EpisodeWatchProgress_Episode_episodeID",
                        column: x => x.episodeID,
                        principalSchema: "movie",
                        principalTable: "Episode",
                        principalColumn: "episodeID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_movieID_createdAt",
                schema: "movie",
                table: "Comment",
                columns: new[] { "movieID", "createdAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_parentID",
                schema: "movie",
                table: "Comment",
                column: "parentID");

            migrationBuilder.CreateIndex(
                name: "IX_Episode_movieID_seasonNumber_episodeNumber",
                schema: "movie",
                table: "Episode",
                columns: new[] { "movieID", "seasonNumber", "episodeNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeSource_episodeID_sourceType_sourceID_language_quality",
                schema: "movie",
                table: "EpisodeSource",
                columns: new[] { "episodeID", "sourceType", "sourceID", "language", "quality" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeWatchProgress_episodeID",
                schema: "movie",
                table: "EpisodeWatchProgress",
                column: "episodeID");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeWatchProgress_episodeSourceID",
                schema: "movie",
                table: "EpisodeWatchProgress",
                column: "episodeSourceID");

            migrationBuilder.CreateIndex(
                name: "IX_EpisodeWatchProgress_userID_episodeID_episodeSourceID",
                schema: "movie",
                table: "EpisodeWatchProgress",
                columns: new[] { "userID", "episodeID", "episodeSourceID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieImage_movieID",
                schema: "movie",
                table: "MovieImage",
                column: "movieID");

            migrationBuilder.CreateIndex(
                name: "IX_MoviePerson_personID",
                schema: "movie",
                table: "MoviePerson",
                column: "personID");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_regionID",
                schema: "movie",
                table: "Movies",
                column: "regionID");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_slug",
                schema: "movie",
                table: "Movies",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieSource_movieID_sourceType_sourceID_language_quality",
                schema: "movie",
                table: "MovieSource",
                columns: new[] { "movieID", "sourceType", "sourceID", "language", "quality" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieTag_movieID_tagID",
                schema: "movie",
                table: "MovieTag",
                columns: new[] { "movieID", "tagID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieTag_tagID",
                schema: "movie",
                table: "MovieTag",
                column: "tagID");

            migrationBuilder.CreateIndex(
                name: "IX_Person_regionID",
                schema: "movie",
                table: "Person",
                column: "regionID");

            migrationBuilder.CreateIndex(
                name: "IX_SavedMovie_movieID",
                schema: "movie",
                table: "SavedMovie",
                column: "movieID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRating_movieID",
                schema: "movie",
                table: "UserRating",
                column: "movieID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProgress_movieID",
                schema: "movie",
                table: "WatchProgress",
                column: "movieID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProgress_MoviesmovieID",
                schema: "movie",
                table: "WatchProgress",
                column: "MoviesmovieID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProgress_sourceID",
                schema: "movie",
                table: "WatchProgress",
                column: "sourceID");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProgress_userID_movieID_sourceID",
                schema: "movie",
                table: "WatchProgress",
                columns: new[] { "userID", "movieID", "sourceID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "EpisodeWatchProgress",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "ImageSource",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "MovieImage",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "MoviePerson",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "MovieTag",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "SavedMovie",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "UserRating",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "WatchProgress",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "EpisodeSource",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "Person",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "MovieSource",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "Episode",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "Movies",
                schema: "movie");

            migrationBuilder.DropTable(
                name: "Region",
                schema: "movie");
        }
    }
}
