using System;
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
                    phoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    googleSub = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isEnabled = table.Column<bool>(type: "bit", nullable: false),
                    recoveryCodes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    consumedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                    avatar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
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
                name: "AuthUserSession",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthPermission",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthRole",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "AuthUser",
                schema: "auth");
        }
    }
}
