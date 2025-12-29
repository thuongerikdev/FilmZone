using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FZ.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AuthV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "scope",
                schema: "auth",
                table: "AuthUser",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "scope",
                schema: "auth",
                table: "AuthRole",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "auth",
                table: "AuthPermission",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "scope",
                schema: "auth",
                table: "AuthPermission",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 10,
                column: "scope",
                value: null);

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 11,
                column: "scope",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scope",
                schema: "auth",
                table: "AuthUser");

            migrationBuilder.DropColumn(
                name: "scope",
                schema: "auth",
                table: "AuthRole");

            migrationBuilder.DropColumn(
                name: "code",
                schema: "auth",
                table: "AuthPermission");

            migrationBuilder.DropColumn(
                name: "scope",
                schema: "auth",
                table: "AuthPermission");
        }
    }
}
