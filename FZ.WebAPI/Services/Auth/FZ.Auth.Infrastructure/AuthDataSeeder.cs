
using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.User;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Constant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure
{
    public static class AuthDataSeeder
    {
        public static async Task SeedPermissionsAsync(AuthDbContext context)
        {
            // =========================================================
            // 1. ĐỒNG BỘ PERMISSION TỪ CODE (CONSTANTS) -> DB
            // =========================================================
            var allConstantPermissions = PermissionConstants.Permissions;
            var existingPermissions = await context.authPermissions.ToListAsync();
            var newPermissionsToAdd = new List<AuthPermission>();

            foreach (var kvp in allConstantPermissions)
            {
                // Chỉ thêm quyền chưa có trong DB
                if (!existingPermissions.Any(p => p.code == kvp.Value))
                {
                    string scope = "user";
                    // Logic đoán scope: Nếu có từ khóa quản lý/xóa/upload/xem tất cả -> Staff
                    // Trừ trường hợp quyền thao tác dữ liệu cá nhân (_own)
                    if ((kvp.Value.Contains("manage") || kvp.Value.Contains("delete") ||
                         kvp.Value.Contains("upload") || kvp.Value.Contains("read_all") ||
                         kvp.Value.Contains(".admin"))
                        && !kvp.Value.Contains("_own"))
                    {
                        scope = "staff";
                    }

                    newPermissionsToAdd.Add(new AuthPermission
                    {
                        // ID tự sinh bởi DB
                        permissionName = kvp.Key,
                        code = kvp.Value,
                        permissionDescription = kvp.Key,
                        scope = scope
                    });
                }
            }

            if (newPermissionsToAdd.Any())
            {
                await context.authPermissions.AddRangeAsync(newPermissionsToAdd);
                await context.SaveChangesAsync();
            }

            // =========================================================
            // 2. ĐỊNH NGHĨA NHÓM QUYỀN (MAPPING LOGIC)
            // =========================================================

            // Guest (Chưa login - Không lưu DB)
            var guestCodes = new HashSet<string> {
                "auth.login", "auth.login_google", "auth.register",
                "auth.forgot_password", "system.health",
                "payment.callback", "subtitle.callback"
            };

            // Customer Free (Quyền cơ bản + Quyền cá nhân)
            var customerFreeCodes = new HashSet<string> {
                "account.mfa_setup", "account.change_password",
                "auth.logout", "auth.refresh", "auth.mfa_verify",
                "comment.read", "episode.read",
                "movie.read_details", "movie.browse",
                "movie_person.read", "movie_tag.read",
                "person.read", "tag.read",
                "user.read_profile", "user.update_profile", "user.read_details",
                "rating.read",
                "plan.read", "price.read", "region.read",
                "search.movie", "search.suggest", "search.person",
                "subscription.read_own", "subscription.cancel",
                "order.read_own", "invoice.read_own",
                "payment.checkout", "image.read"
            };

            // Customer VIP (Quyền nâng cao)
            var customerVipCodes = new HashSet<string> {
                "comment.create", "comment.update_own", "comment.delete_own",
                "source.read", "progress.track", "progress.read",
                "movie.watch_stream", "movie.watch_vip", "subtitle.read",
                "saved_movie.manage", "saved_movie.read",
                "rating.create", "rating.update", "rating.delete"
            };

            // Staff (Quản lý - Danh sách các tính năng quản lý chung)
            var staffCodes = new HashSet<string> {
                "upload.archive", "upload.vimeo", "upload.youtube",
                "episode.manage", "source.manage", "image.manage",
                "invoice.read_all", "movie.manage", "movie_person.manage", "movie_tag.manage",
                "subtitle.upload", "subtitle.translate", "subtitle.manage",
                "order.read_all", "permission.read", "person.manage",
                "plan.manage", "price.manage", "region.manage",
                "subscription.read_all", "subscription.manage", "tag.manage",
                "user.read_list", "user.read_details",
                "search.advanced",
                "role.assign", "permission.assign" 
                // Lưu ý: KHÔNG liệt kê các quyền .admin vào đây
            };

            // =========================================================
            // 3. GÁN QUYỀN VÀO ROLE (SYNC ROLE-PERMISSION)
            // =========================================================

            // Lấy dữ liệu mới nhất từ DB
            var allPermissionsInDb = await context.authPermissions.ToListAsync();
            var allRolePermissionsInDb = await context.authRolePermissions.ToListAsync();

            // ID Role (Tương ứng với file Seed trong DbContext)
            int adminRoleId = 1;
            int contentMgrId = 2;
            int userMgrId = 3;
            int financeMgrId = 4;
            int customerId = 10;
            int vipId = 11;

            var linksToAdd = new List<AuthRolePermission>();

            // Helper check tồn tại để tránh Add trùng
            void AddLinkIfNotExist(int rId, int pId)
            {
                bool existsInDb = allRolePermissionsInDb.Any(rp => rp.roleID == rId && rp.permissionID == pId);
                bool existsInPending = linksToAdd.Any(rp => rp.roleID == rId && rp.permissionID == pId);

                if (!existsInDb && !existsInPending)
                {
                    linksToAdd.Add(new AuthRolePermission { roleID = rId, permissionID = pId });
                }
            }

            foreach (var perm in allPermissionsInDb)
            {
                if (guestCodes.Contains(perm.code)) continue;

                // -----------------------------------------------------
                // 1. ADMIN (ID 1): NHẬN TẤT CẢ (Bao gồm cả .admin)
                // -----------------------------------------------------
                AddLinkIfNotExist(adminRoleId, perm.permissionID);


                // -----------------------------------------------------
                // 2. STAFF ROLES (ID 2, 3, 4)
                // -----------------------------------------------------
                // Logic: Là quyền trong nhóm Staff HOẶC chứa từ khóa quản lý
                // QUAN TRỌNG: Loại bỏ tất cả quyền có đuôi ".admin"

                bool isStaffPerm = staffCodes.Contains(perm.code) ||
                                   (perm.code.Contains("manage") || perm.code.Contains("read_list"));

                if (isStaffPerm && !perm.code.EndsWith(".admin"))
                {
                    // A. Content Manager (Nội dung)
                    if (perm.code.StartsWith("movie") || perm.code.StartsWith("episode") ||
                        perm.code.StartsWith("person") || perm.code.StartsWith("tag") ||
                        perm.code.StartsWith("image") || perm.code.StartsWith("source") ||
                        perm.code.StartsWith("subtitle") || perm.code.StartsWith("upload"))
                    {
                        AddLinkIfNotExist(contentMgrId, perm.permissionID);
                    }

                    // B. User Manager (Người dùng)
                    if (perm.code.StartsWith("user") ||
                        perm.code.StartsWith("role.assign") ||
                        perm.code.StartsWith("permission.assign"))
                    {
                        AddLinkIfNotExist(userMgrId, perm.permissionID);
                    }

                    // C. Finance Manager (Tài chính)
                    if (perm.code.StartsWith("order") || perm.code.StartsWith("invoice") ||
                        perm.code.StartsWith("plan") || perm.code.StartsWith("price") ||
                        perm.code.StartsWith("subscription"))
                    {
                        AddLinkIfNotExist(financeMgrId, perm.permissionID);
                    }

                    // D. Shared Staff Permissions (Quyền chung cho cả 3 ông)
                    if (perm.code == "permission.read" || perm.code == "search.advanced")
                    {
                        AddLinkIfNotExist(contentMgrId, perm.permissionID);
                        AddLinkIfNotExist(userMgrId, perm.permissionID);
                        AddLinkIfNotExist(financeMgrId, perm.permissionID);
                    }
                }


                // -----------------------------------------------------
                // 3. CUSTOMER FREE & BASIC STAFF RIGHTS
                // -----------------------------------------------------
                if (customerFreeCodes.Contains(perm.code))
                {
                    AddLinkIfNotExist(customerId, perm.permissionID);
                    AddLinkIfNotExist(vipId, perm.permissionID);

                    // Staff cũng cần quyền cơ bản (đổi pass, logout, xem profile mình)
                    AddLinkIfNotExist(contentMgrId, perm.permissionID);
                    AddLinkIfNotExist(userMgrId, perm.permissionID);
                    AddLinkIfNotExist(financeMgrId, perm.permissionID);
                }

                // -----------------------------------------------------
                // 4. CUSTOMER VIP
                // -----------------------------------------------------
                if (customerVipCodes.Contains(perm.code))
                {
                    AddLinkIfNotExist(vipId, perm.permissionID);
                }
            }

            if (linksToAdd.Any())
            {
                await context.authRolePermissions.AddRangeAsync(linksToAdd);
                await context.SaveChangesAsync();
            }
        }

        // =========================================================
        // SEED ADMIN USER
        // =========================================================
        public static async Task SeedAdminUserAsync(AuthDbContext context, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeederAdmin");

            try
            {
                var adminEmail = "admin@fz.com";

                // 1. Kiểm tra User tồn tại (Bỏ qua filter xóa mềm nếu có)
                var adminUserExists = await context.authUsers.IgnoreQueryFilters().AnyAsync(u => u.email == adminEmail);
                if (adminUserExists)
                {
                    logger.LogInformation("⚠️ Admin user already exists. Skipping.");
                    return;
                }

                // 2. Tìm Role Admin an toàn
                var adminRole = await context.authRoles.FirstOrDefaultAsync(r => r.roleName == "admin");

                if (adminRole == null)
                {
                    logger.LogError("❌ Critical Error: Role 'admin' not found in DB. Please run Migrations first.");
                    // Tự động tạo Role chữa cháy nếu thiếu để không crash app
                    adminRole = new AuthRole { roleName = "admin", roleDescription = "Admin (Auto Generated)", scope = "staff" };
                    context.authRoles.Add(adminRole);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Created missing 'admin' role.");
                }

                // 3. Hash Password
                var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
                string rawPassword = "Admin@123"; // Mật khẩu mặc định
                string hashedPassword = passwordHasher.Hash(rawPassword);

                // 4. Tạo User
                var adminUser = new AuthUser
                {
                    userName = "admin",
                    email = adminEmail,
                    phoneNumber = "0999999999",
                    passwordHash = hashedPassword,
                    isEmailVerified = true,
                    status = "active",
                    tokenVersion = 1,
                    scope = "staff",
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow,
                    profile = new AuthProfile
                    {
                        firstName = "System",
                        lastName = "Administrator",
                        gender = "other",
                        dateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        avatar = "https://ui-avatars.com/api/?name=System+Admin"
                    }
                };

                context.authUsers.Add(adminUser);
                await context.SaveChangesAsync();
                logger.LogInformation($"✅ Created Admin User (ID: {adminUser.userID})");

                // 5. Gán Role
                var adminRoleLink = new AuthUserRole
                {
                    userID = adminUser.userID,
                    roleID = adminRole.roleID,
                    assignedAt = DateTime.UtcNow
                };

                context.authUserRoles.Add(adminRoleLink);
                await context.SaveChangesAsync();
                logger.LogInformation("✅ Assigned Admin Role to User.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to seed Admin User");
                throw; // Ném lỗi ra để AuthStartUp bắt được và dừng app nếu cần
            }
        }
    }
}