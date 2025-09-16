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
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Plan_planID1",
                schema: "auth",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Price_priceID1",
                schema: "auth",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_planID1",
                schema: "auth",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_priceID1",
                schema: "auth",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "planID1",
                schema: "auth",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "priceID1",
                schema: "auth",
                table: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_provider_providerPaymentId",
                schema: "auth",
                table: "Payment",
                columns: new[] { "provider", "providerPaymentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payment_provider_providerPaymentId",
                schema: "auth",
                table: "Payment");

            migrationBuilder.AddColumn<int>(
                name: "planID1",
                schema: "auth",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "priceID1",
                schema: "auth",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_planID1",
                schema: "auth",
                table: "Order",
                column: "planID1");

            migrationBuilder.CreateIndex(
                name: "IX_Order_priceID1",
                schema: "auth",
                table: "Order",
                column: "priceID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Plan_planID1",
                schema: "auth",
                table: "Order",
                column: "planID1",
                principalSchema: "auth",
                principalTable: "Plan",
                principalColumn: "planID");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Price_priceID1",
                schema: "auth",
                table: "Order",
                column: "priceID1",
                principalSchema: "auth",
                principalTable: "Price",
                principalColumn: "priceID");
        }
    }
}
