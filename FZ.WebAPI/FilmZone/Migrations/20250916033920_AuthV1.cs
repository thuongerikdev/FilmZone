using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FZ.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AuthV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "AuthPermission",
                schema: "auth",
                columns: table => new
                {
                    permissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    permissionName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    permissionDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthPermission", x => x.permissionID);
                });

            migrationBuilder.CreateTable(
                name: "AuthRole",
                schema: "auth",
                columns: table => new
                {
                    roleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    roleDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    isDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRole", x => x.roleID);
                });

            migrationBuilder.CreateTable(
                name: "AuthUser",
                schema: "auth",
                columns: table => new
                {
                    userID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    googleSub = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    tokenVersion = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthUser", x => x.userID);
                });

            migrationBuilder.CreateTable(
                name: "Plan",
                schema: "auth",
                columns: table => new
                {
                    planID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.planID);
                });

            migrationBuilder.CreateTable(
                name: "AuthRolePermission",
                schema: "auth",
                columns: table => new
                {
                    rolePermissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    roleID = table.Column<int>(type: "int", nullable: false),
                    permissionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRolePermission", x => x.rolePermissionID);
                    table.ForeignKey(
                        name: "FK_AuthRolePermission_AuthPermission_permissionID",
                        column: x => x.permissionID,
                        principalSchema: "auth",
                        principalTable: "AuthPermission",
                        principalColumn: "permissionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthRolePermission_AuthRole_roleID",
                        column: x => x.roleID,
                        principalSchema: "auth",
                        principalTable: "AuthRole",
                        principalColumn: "roleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthAuditLog",
                schema: "auth",
                columns: table => new
                {
                    auditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    detail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthAuditLog", x => x.auditID);
                    table.ForeignKey(
                        name: "FK_AuthAuditLog_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID");
                });

            migrationBuilder.CreateTable(
                name: "AuthEmailVerification",
                schema: "auth",
                columns: table => new
                {
                    emailVerificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    codeHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    consumedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthEmailVerification", x => x.emailVerificationID);
                    table.ForeignKey(
                        name: "FK_AuthEmailVerification_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthMfaSecret",
                schema: "auth",
                columns: table => new
                {
                    mfaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isEnabled = table.Column<bool>(type: "bit", nullable: false),
                    label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    recoveryCodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    enrollmentStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    enabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lastVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthMfaSecret", x => x.mfaID);
                    table.ForeignKey(
                        name: "FK_AuthMfaSecret_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthPasswordReset",
                schema: "auth",
                columns: table => new
                {
                    passwordResetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    codeHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    consumedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    purpose = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthPasswordReset", x => x.passwordResetID);
                    table.ForeignKey(
                        name: "FK_AuthPasswordReset_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthProfile",
                schema: "auth",
                columns: table => new
                {
                    profileID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    dateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthProfile", x => x.profileID);
                    table.ForeignKey(
                        name: "FK_AuthProfile_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthUserRole",
                schema: "auth",
                columns: table => new
                {
                    userID = table.Column<int>(type: "int", nullable: false),
                    roleID = table.Column<int>(type: "int", nullable: false),
                    assignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthUserRole", x => x.userID);
                    table.ForeignKey(
                        name: "FK_AuthUserRole_AuthRole_roleID",
                        column: x => x.roleID,
                        principalSchema: "auth",
                        principalTable: "AuthRole",
                        principalColumn: "roleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthUserRole_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthUserSession",
                schema: "auth",
                columns: table => new
                {
                    sessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    deviceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    lastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthUserSession", x => x.sessionID);
                    table.ForeignKey(
                        name: "FK_AuthUserSession_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Price",
                schema: "auth",
                columns: table => new
                {
                    priceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    planID = table.Column<int>(type: "int", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    intervalUnit = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    intervalCount = table.Column<int>(type: "int", nullable: false),
                    trialDays = table.Column<int>(type: "int", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Price", x => x.priceID);
                    table.ForeignKey(
                        name: "FK_Price_Plan_planID",
                        column: x => x.planID,
                        principalSchema: "auth",
                        principalTable: "Plan",
                        principalColumn: "planID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthRefreshToken",
                schema: "auth",
                columns: table => new
                {
                    refreshTokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    sessionID = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Revoked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthRefreshToken", x => x.refreshTokenID);
                    table.ForeignKey(
                        name: "FK_AuthRefreshToken_AuthUserSession_sessionID",
                        column: x => x.sessionID,
                        principalSchema: "auth",
                        principalTable: "AuthUserSession",
                        principalColumn: "sessionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthRefreshToken_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                schema: "auth",
                columns: table => new
                {
                    orderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    planID = table.Column<int>(type: "int", nullable: false),
                    priceID = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    providerSessionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    planID1 = table.Column<int>(type: "int", nullable: true),
                    priceID1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.orderID);
                    table.ForeignKey(
                        name: "FK_Order_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Plan_planID",
                        column: x => x.planID,
                        principalSchema: "auth",
                        principalTable: "Plan",
                        principalColumn: "planID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Plan_planID1",
                        column: x => x.planID1,
                        principalSchema: "auth",
                        principalTable: "Plan",
                        principalColumn: "planID");
                    table.ForeignKey(
                        name: "FK_Order_Price_priceID",
                        column: x => x.priceID,
                        principalSchema: "auth",
                        principalTable: "Price",
                        principalColumn: "priceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Order_Price_priceID1",
                        column: x => x.priceID1,
                        principalSchema: "auth",
                        principalTable: "Price",
                        principalColumn: "priceID");
                });

            migrationBuilder.CreateTable(
                name: "UserSubscription",
                schema: "auth",
                columns: table => new
                {
                    subscriptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    planID = table.Column<int>(type: "int", nullable: false),
                    priceID = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    autoRenew = table.Column<bool>(type: "bit", nullable: false),
                    startAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    trialEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    currentPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    currentPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    cancelAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelAtPeriodEnd = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscription", x => x.subscriptionID);
                    table.ForeignKey(
                        name: "FK_UserSubscription_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscription_Plan_planID",
                        column: x => x.planID,
                        principalSchema: "auth",
                        principalTable: "Plan",
                        principalColumn: "planID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscription_Price_priceID",
                        column: x => x.priceID,
                        principalSchema: "auth",
                        principalTable: "Price",
                        principalColumn: "priceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                schema: "auth",
                columns: table => new
                {
                    invoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    subscriptionID = table.Column<int>(type: "int", nullable: true),
                    orderID = table.Column<int>(type: "int", nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    issuedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    pdfUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.invoiceID);
                    table.ForeignKey(
                        name: "FK_Invoice_AuthUser_userID",
                        column: x => x.userID,
                        principalSchema: "auth",
                        principalTable: "AuthUser",
                        principalColumn: "userID");
                    table.ForeignKey(
                        name: "FK_Invoice_Order_orderID",
                        column: x => x.orderID,
                        principalSchema: "auth",
                        principalTable: "Order",
                        principalColumn: "orderID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoice_UserSubscription_subscriptionID",
                        column: x => x.subscriptionID,
                        principalSchema: "auth",
                        principalTable: "UserSubscription",
                        principalColumn: "subscriptionID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "auth",
                columns: table => new
                {
                    paymentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoiceID = table.Column<int>(type: "int", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    providerPaymentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    paidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    failureReason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.paymentID);
                    table.ForeignKey(
                        name: "FK_Payment_Invoice_invoiceID",
                        column: x => x.invoiceID,
                        principalSchema: "auth",
                        principalTable: "Invoice",
                        principalColumn: "invoiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "Plan",
                columns: new[] { "planID", "code", "description", "isActive", "name" },
                values: new object[] { 1, "VIP", "Quyền lợi VIP (không quảng cáo, chất lượng cao...)", true, "Gói VIP" });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "Price",
                columns: new[] { "priceID", "amount", "currency", "intervalCount", "intervalUnit", "isActive", "planID", "trialDays" },
                values: new object[,]
                {
                    { 101, 99000m, "VND", 1, "month", true, 1, null },
                    { 102, 249000m, "VND", 3, "month", true, 1, null },
                    { 103, 459000m, "VND", 6, "month", true, 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthAuditLog_userID",
                schema: "auth",
                table: "AuthAuditLog",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthEmailVerification_userID",
                schema: "auth",
                table: "AuthEmailVerification",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthMfaSecret_userID",
                schema: "auth",
                table: "AuthMfaSecret",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthMfaSecret_userID_type",
                schema: "auth",
                table: "AuthMfaSecret",
                columns: new[] { "userID", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthPasswordReset_userID",
                schema: "auth",
                table: "AuthPasswordReset",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthProfile_userID",
                schema: "auth",
                table: "AuthProfile",
                column: "userID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthRefreshToken_sessionID",
                schema: "auth",
                table: "AuthRefreshToken",
                column: "sessionID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthRefreshToken_userID",
                schema: "auth",
                table: "AuthRefreshToken",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthRolePermission_permissionID",
                schema: "auth",
                table: "AuthRolePermission",
                column: "permissionID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthRolePermission_roleID",
                schema: "auth",
                table: "AuthRolePermission",
                column: "roleID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthUserRole_roleID",
                schema: "auth",
                table: "AuthUserRole",
                column: "roleID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthUserSession_userID",
                schema: "auth",
                table: "AuthUserSession",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_orderID",
                schema: "auth",
                table: "Invoice",
                column: "orderID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_subscriptionID",
                schema: "auth",
                table: "Invoice",
                column: "subscriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_userID_issuedAt",
                schema: "auth",
                table: "Invoice",
                columns: new[] { "userID", "issuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_planID",
                schema: "auth",
                table: "Order",
                column: "planID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_planID1",
                schema: "auth",
                table: "Order",
                column: "planID1");

            migrationBuilder.CreateIndex(
                name: "IX_Order_priceID",
                schema: "auth",
                table: "Order",
                column: "priceID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_priceID1",
                schema: "auth",
                table: "Order",
                column: "priceID1");

            migrationBuilder.CreateIndex(
                name: "IX_Order_provider_providerSessionId",
                schema: "auth",
                table: "Order",
                columns: new[] { "provider", "providerSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_userID",
                schema: "auth",
                table: "Order",
                column: "userID");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_invoiceID",
                schema: "auth",
                table: "Payment",
                column: "invoiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Plan_code",
                schema: "auth",
                table: "Plan",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Price_planID_currency_intervalUnit_intervalCount",
                schema: "auth",
                table: "Price",
                columns: new[] { "planID", "currency", "intervalUnit", "intervalCount" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_currentPeriodEnd",
                schema: "auth",
                table: "UserSubscription",
                column: "currentPeriodEnd");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_planID",
                schema: "auth",
                table: "UserSubscription",
                column: "planID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_priceID",
                schema: "auth",
                table: "UserSubscription",
                column: "priceID");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscription_userID_planID_status",
                schema: "auth",
                table: "UserSubscription",
                columns: new[] { "userID", "planID", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthAuditLog",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthEmailVerification",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthMfaSecret",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthPasswordReset",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthProfile",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthRefreshToken",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthRolePermission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthUserRole",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Payment",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthUserSession",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthPermission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthRole",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Invoice",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Order",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "UserSubscription",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthUser",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Price",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Plan",
                schema: "auth");
        }
    }
}
