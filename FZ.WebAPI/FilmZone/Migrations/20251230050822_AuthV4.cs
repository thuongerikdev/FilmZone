using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FZ.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AuthV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 10,
                columns: new[] { "roleDescription", "scope" },
                values: new object[] { "Khách hàng", "user" });

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 11,
                columns: new[] { "roleDescription", "scope" },
                values: new object[] { "Khách hàng VIP", "user" });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "AuthRole",
                columns: new[] { "roleID", "isDefault", "roleDescription", "roleName", "scope" },
                values: new object[,]
                {
                    { 1, false, "Quản trị viên", "admin", "staff" },
                    { 2, false, "Quản lý nội dung", "content_manager", "staff" },
                    { 3, false, "Quản lý người dùng", "user_manager", "staff" },
                    { 4, false, "Quản lý tài chính", "finance_manager", "staff" }
                });

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Plan",
                keyColumn: "planID",
                keyValue: 1,
                column: "description",
                value: "Quyền lợi VIP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 4);

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 10,
                columns: new[] { "roleDescription", "scope" },
                values: new object[] { "Khách hàng tiêu chuẩn", null });

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "AuthRole",
                keyColumn: "roleID",
                keyValue: 11,
                columns: new[] { "roleDescription", "scope" },
                values: new object[] { "Khách hàng VIP (đồng bộ với gói VIP)", null });

            migrationBuilder.UpdateData(
                schema: "auth",
                table: "Plan",
                keyColumn: "planID",
                keyValue: 1,
                column: "description",
                value: "Quyền lợi VIP (không quảng cáo, chất lượng cao...)");
        }
    }
}
