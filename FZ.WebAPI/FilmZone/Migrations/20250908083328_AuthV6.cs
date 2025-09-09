using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FZ.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AuthV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "purpose",
                schema: "auth",
                table: "AuthPasswordReset",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "secret",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "recoveryCodes",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "enabledAt",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "enrollmentStartedAt",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "label",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lastVerifiedAt",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AuthMfaSecret_userID_type",
                schema: "auth",
                table: "AuthMfaSecret",
                columns: new[] { "userID", "type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuthMfaSecret_userID_type",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.DropColumn(
                name: "purpose",
                schema: "auth",
                table: "AuthPasswordReset");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.DropColumn(
                name: "enabledAt",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.DropColumn(
                name: "enrollmentStartedAt",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.DropColumn(
                name: "label",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.DropColumn(
                name: "lastVerifiedAt",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "auth",
                table: "AuthMfaSecret");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "secret",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "recoveryCodes",
                schema: "auth",
                table: "AuthMfaSecret",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
