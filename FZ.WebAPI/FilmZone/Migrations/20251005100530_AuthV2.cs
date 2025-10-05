using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FZ.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AuthV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthUserRole",
                schema: "auth",
                table: "AuthUserRole");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthUserRole",
                schema: "auth",
                table: "AuthUserRole",
                columns: new[] { "userID", "roleID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthUserRole",
                schema: "auth",
                table: "AuthUserRole");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthUserRole",
                schema: "auth",
                table: "AuthUserRole",
                column: "userID");
        }
    }
}
