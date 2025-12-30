using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.User;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Constant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure
{
    public static class AuthDataSeeder
    {
        public static async Task SeedPermissionsAsync(AuthDbContext context)
        {
            // 1. Đồng bộ Permission từ Constant vào Database
            // (Chỉ thêm mới nếu chưa có, KHÔNG xóa quyền cũ để an toàn dữ liệu)
            var allConstantPermissions = PermissionConstants.Permissions;
            var existingPermissions = await context.authPermissions.ToListAsync();
            var newPermissionsToAdd = new List<AuthPermission>();

            foreach (var kvp in allConstantPermissions)
            {
                if (!existingPermissions.Any(p => p.code == kvp.Value))
                {
                    string scope = "user";
                    if ((kvp.Value.Contains("manage") || kvp.Value.Contains("delete") || kvp.Value.Contains("upload") || kvp.Value.Contains("read_all"))
                        && !kvp.Value.Contains("_own"))
                    {
                        scope = "staff";
                    }

                    newPermissionsToAdd.Add(new AuthPermission
                    {
                        // ID tự sinh, không set cứng
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
                await context.SaveChangesAsync(); // Lưu để lấy ID
            }

            // 2. Định nghĩa các nhóm quyền (Mapping Logic)
            var guestCodes = new HashSet<string> { "auth.login", "auth.login_google", "auth.register", "auth.forgot_password", "system.health", "payment.callback", "subtitle.callback" };

            var customerFreeCodes = new HashSet<string> {
                "account.mfa_setup", "account.change_password", "auth.logout", "auth.refresh", "auth.mfa_verify",
                "comment.read", "episode.read", "movie.read_details", "movie.browse", "movie_person.read", "movie_tag.read",
                "person.read", "tag.read", "user.read_profile", "user.update_profile", "rating.read",
                "plan.read", "price.read", "region.read", "search.movie", "search.suggest", "search.person",
                "subscription.read_own", "subscription.cancel", "order.read_own", "invoice.read_own", "payment.checkout", "image.read"
            };

            var customerVipCodes = new HashSet<string> {
                "comment.create", "comment.update_own", "comment.delete_own", "source.read", "progress.track", "progress.read",
                "movie.watch_stream", "movie.watch_vip", "subtitle.read", "saved_movie.manage", "saved_movie.read",
                "rating.create", "rating.update", "rating.delete"
            };

            var staffCodes = new HashSet<string> {
                "upload.archive", "upload.vimeo", "upload.youtube", "episode.manage", "source.manage", "image.manage",
                "invoice.read_all", "movie.manage", "movie_person.manage", "movie_tag.manage", "subtitle.upload", "subtitle.translate", "subtitle.manage",
                "order.read_all", "permission.read", "person.manage", "plan.manage", "price.manage", "region.manage",
                "subscription.read_all", "subscription.manage", "tag.manage", "user.read_list", "user.read_details", "search.advanced"
            };

            // 3. Gán quyền vào Role (Sync RolePermission)
            // Lấy dữ liệu mới nhất từ DB
            var allPermissionsInDb = await context.authPermissions.ToListAsync();
            var allRolePermissionsInDb = await context.authRolePermissions.ToListAsync();

            // Lấy ID của các Role (Đã seed trong DbContext)
            int adminRoleId = 1;
            int contentMgrId = 2;
            int userMgrId = 3;
            int financeMgrId = 4;
            int customerId = 10;
            int vipId = 11;

            var linksToAdd = new List<AuthRolePermission>();

            foreach (var perm in allPermissionsInDb)
            {
                if (guestCodes.Contains(perm.code)) continue;

                // --- Helper: Hàm check xem quyền đã được gán chưa để tránh trùng lặp ---
                void AddLinkIfNotExist(int rId, int pId)
                {
                    bool existsInDb = allRolePermissionsInDb.Any(rp => rp.roleID == rId && rp.permissionID == pId);
                    bool existsInPending = linksToAdd.Any(rp => rp.roleID == rId && rp.permissionID == pId);

                    if (!existsInDb && !existsInPending)
                    {
                        linksToAdd.Add(new AuthRolePermission { roleID = rId, permissionID = pId });
                    }
                }

                // 1. ADMIN: Full quyền
                AddLinkIfNotExist(adminRoleId, perm.permissionID);

                // 2. CUSTOMER FREE
                if (customerFreeCodes.Contains(perm.code))
                {
                    AddLinkIfNotExist(customerId, perm.permissionID);
                    // VIP cũng có quyền Free
                    AddLinkIfNotExist(vipId, perm.permissionID);
                    // Staff cũng cần quyền cơ bản của User
                    AddLinkIfNotExist(contentMgrId, perm.permissionID);
                    AddLinkIfNotExist(userMgrId, perm.permissionID);
                    AddLinkIfNotExist(financeMgrId, perm.permissionID);
                }

                // 3. CUSTOMER VIP (Chỉ VIP có thêm)
                if (customerVipCodes.Contains(perm.code))
                {
                    AddLinkIfNotExist(vipId, perm.permissionID);
                }

                // 4. STAFF ROLES
                if (staffCodes.Contains(perm.code))
                {
                    // Phân quyền chi tiết
                    // Content Manager
                    if (perm.code.StartsWith("movie") || perm.code.StartsWith("episode") || perm.code.StartsWith("person") ||
                        perm.code.StartsWith("tag") || perm.code.StartsWith("image") || perm.code.StartsWith("source") ||
                        perm.code.StartsWith("subtitle") || perm.code.StartsWith("upload"))
                    {
                        AddLinkIfNotExist(contentMgrId, perm.permissionID);
                    }

                    // User Manager
                    if (perm.code.StartsWith("user"))
                    {
                        AddLinkIfNotExist(userMgrId, perm.permissionID);
                    }

                    // Finance Manager
                    if (perm.code.StartsWith("order") || perm.code.StartsWith("invoice") || perm.code.StartsWith("plan") ||
                        perm.code.StartsWith("price") || perm.code.StartsWith("subscription"))
                    {
                        AddLinkIfNotExist(financeMgrId, perm.permissionID);
                    }

                    // Shared Staff Permissions
                    if (perm.code == "permission.read" || perm.code == "search.advanced")
                    {
                        AddLinkIfNotExist(contentMgrId, perm.permissionID);
                        AddLinkIfNotExist(userMgrId, perm.permissionID);
                        AddLinkIfNotExist(financeMgrId, perm.permissionID);
                    }
                }
            }

            if (linksToAdd.Any())
            {
                await context.authRolePermissions.AddRangeAsync(linksToAdd);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedAdminUserAsync(AuthDbContext context, IServiceProvider serviceProvider)
        {
            // Lấy logger để in ra console cho dễ nhìn thấy lỗi
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeederAdmin");

            try
            {
                var adminEmail = "admin@fz.com";

                // 1. Kiểm tra User tồn tại
                var adminUserExists = await context.authUsers.IgnoreQueryFilters().AnyAsync(u => u.email == adminEmail);
                if (adminUserExists)
                {
                    logger.LogInformation("⚠️ Admin user already exists. Skipping.");
                    return;
                }

                // 2. Tìm Role Admin (QUAN TRỌNG: Không gán cứng ID = 1)
                // Tìm theo tên để an toàn hơn, phòng trường hợp ID trong DB bị nhảy
                var adminRole = await context.authRoles.FirstOrDefaultAsync(r => r.roleName == "admin");

                if (adminRole == null)
                {
                    logger.LogError("❌ Critical Error: Role 'admin' not found in DB. Please run Migrations first.");
                    // Tùy chọn: Nếu chưa có Role thì tạo luôn Role (chữa cháy)
                    adminRole = new AuthRole { roleName = "admin", roleDescription = "Admin (Auto Generated)", scope = "staff" };
                    context.authRoles.Add(adminRole);
                    await context.SaveChangesAsync();
                    logger.LogInformation("✅ Created missing 'admin' role.");
                }

                // 3. Hash Password
                var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
                string rawPassword = "admin";
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
                    roleID = adminRole.roleID, // Dùng ID thật lấy từ DB
                    assignedAt = DateTime.UtcNow
                };

                context.authUserRoles.Add(adminRoleLink);
                await context.SaveChangesAsync();
                logger.LogInformation("✅ Assigned Admin Role to User.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to seed Admin User");
                throw; // Ném lỗi ra để AuthStartUp bắt được
            }
        }
    }
    
}