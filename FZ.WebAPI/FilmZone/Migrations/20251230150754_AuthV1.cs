using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FZ.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AuthV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "roleID",
                schema: "auth",
                table: "Plan",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Plan",
                keyColumn: "planID",
                keyValue: 1,
                column: "roleID",
                value: 11);

            migrationBuilder.CreateIndex(
                name: "IX_Plan_roleID",
                schema: "auth",
                table: "Plan",
                column: "roleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Plan_AuthRole_roleID",
                schema: "auth",
                table: "Plan",
                column: "roleID",
                principalSchema: "auth",
                principalTable: "AuthRole",
                principalColumn: "roleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plan_AuthRole_roleID",
                schema: "auth",
                table: "Plan");

            migrationBuilder.DropIndex(
                name: "IX_Plan_roleID",
                schema: "auth",
                table: "Plan");

            migrationBuilder.DropColumn(
                name: "roleID",
                schema: "auth",
                table: "Plan");
        }
    }
}
