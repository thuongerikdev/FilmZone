using FZ.Auth.ApplicationService.Common;
using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.Domain.Role;
using FZ.Auth.Domain.Token;
using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Constant;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.MFAService.Implements.User
{
    public sealed class AuthRegisterService : AuthServiceBase  ,IAuthRegisterService
    {
        private readonly IUserRepository _users;
        private readonly IProfileRepository _profiles;
        private readonly IPasswordHasher _hasher;
        private readonly IEmailTokenRepository _emailTokenRepository;
        private readonly IEmailService _emailService;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _uow;

        public AuthRegisterService(IUserRepository users,
                           IProfileRepository profiles,
                           IPasswordHasher hasher,
                           IEmailTokenRepository emailTokenRepository,
                            IEmailService emailService,
                            IRoleRepository roleRepository,
                            IUserRoleRepository userRoleRepository,
                            ILogger<AuthRegisterService> logger,
                           IUnitOfWork uow) : base(logger)
        {
            _users = users;
            _profiles = profiles;
            _hasher = hasher;
            _uow = uow;
            _emailTokenRepository = emailTokenRepository;
            _emailService = emailService;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;

        }

        private string GenerateVerificationToken() => Guid.NewGuid().ToString();


        public Task<ResponseDto<RegisterResponse>> RegisterAsync(RegisterRequest req, CancellationToken ct)
        => _uow.ExecuteInTransactionAsync(async _ =>
        {
            // (1) Validate sơ bộ (nếu dùng FluentValidation thì validate trước khi gọi Service)
            if (string.IsNullOrWhiteSpace(req.userName) ||
                string.IsNullOrWhiteSpace(req.email) ||
                string.IsNullOrWhiteSpace(req.password))
            {
                return ResponseConst.Error<RegisterResponse>(400, "Thiếu thông tin");
            }

            // (2) Chuẩn hóa
            var userName = req.userName.Trim();
            var email = req.email.Trim().ToLowerInvariant();

            // (3) Kiểm tra trùng
            if (await _users.ExistsByUserNameAsync(userName, ct))
                return ResponseConst.Error<RegisterResponse>(409, "UserName đã tồn tại");
            if (await _users.ExistsByEmailAsync(email, ct))
                return ResponseConst.Error<RegisterResponse>(409, "Email đã tồn tại");



            // (4) Tạo entity
            var user = new AuthUser
            {
                userName = userName,
                email = email,
                passwordHash = _hasher.Hash(req.password),
                isEmailVerified = false,
                status = "Active",
                tokenVersion = 1,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct); // Lưu user trước để có userID


            var defaultRole = await _roleRepository.GetDefaultRoleAsync(ct);
            if (defaultRole == null)
                return ResponseConst.Error<RegisterResponse>(500, "Chưa có role mặc định, vui lòng liên hệ quản trị viên");
            // (3.1) Nếu có role mặc định, gán cho user
            var userRole = new AuthUserRole
            {
                role = defaultRole,
                userID =user.userID,
                assignedAt = DateTime.UtcNow
            };
            await _userRoleRepository.AddUserRoleAsync(userRole, ct);
            await _uow.SaveChangesAsync(ct); // Lưu user trước để có userID


            var verifyToken = GenerateVerificationToken();
            var tokenHash = _hasher.Hash(verifyToken);
           

            var emailVerification = new AuthEmailVerification
            {
                user = user,
                codeHash = tokenHash,
                expiresAt = DateTime.UtcNow.AddHours(24),
                createdAt = DateTime.UtcNow,
                consumedAt = null
            };
            await _emailTokenRepository.AddTokenAsync(emailVerification, ct);
            // Gửi email xác thực
            await _emailService.SendVerificationEmail(user.userID, user.email, verifyToken);

            // (5) (Tuỳ chọn) tạo profile
            if (!string.IsNullOrWhiteSpace(req.firstName) || !string.IsNullOrWhiteSpace(req.lastName))
            {
                var profile = new AuthProfile
                {
                    userID = user.userID,
                    firstName = req.firstName,
                    lastName = req.lastName,
                    gender  = req.gender,
                };
                await _profiles.AddAsync(profile, ct);
            }

            // (6) Commit
            await _uow.SaveChangesAsync(ct);

            // (7) Trả Response
            return ResponseConst.Success("Đăng ký thành công", new RegisterResponse
            {
                userID = user.userID,
                userName = user.userName,
                email = user.email,
                isEmailVerified = user.isEmailVerified
            });
        }, ct: ct);

        public async Task<ResponseDto<bool>> VerifyEmailAsync(VerifyEmailRequest req, CancellationToken ct)
        {
            // (0) Validate input nhanh, có thể làm ngoài transaction
            if (req.userID <= 0 || string.IsNullOrWhiteSpace(req.token))
                return ResponseConst.Error<bool>(400, "Thiếu thông tin");

            // GÓI TOÀN BỘ VÀO TRANSACTION (đọc + ghi)
            return await _uow.ExecuteInTransactionAsync(async innerCt =>
            {
                // (1) Tìm user (nên đọc trong transaction để tránh race)
                var user = await _users.FindByIdAsync(req.userID, innerCt);
                if (user == null)
                    return ResponseConst.Error<bool>(404, "Không tìm thấy user");

                if (user.isEmailVerified)
                    return ResponseConst.Success("Email đã được xác thực trước đó", true);

                // (2) Kiểm tra token hợp lệ (và còn hạn)
                // Gợi ý: hàm verifyEmail nên trả về thêm thông tin token để bạn có thể "consume" (xóa/đánh dấu đã dùng)
                var tokenInfo = await _emailTokenRepository.verifyEmail(req, innerCt);
                if (tokenInfo == null)
                    return ResponseConst.Error<bool>(400, "Token không hợp lệ hoặc đã hết hạn");
                if (tokenInfo.expiresAt < DateTime.UtcNow)
                    return ResponseConst.Error<bool>(400, "Token đã hết hạn");
      

                // (3) Cập nhật trạng thái user
                user.isEmailVerified = true;
                user.updatedAt = DateTime.UtcNow;

                tokenInfo.consumedAt = DateTime.UtcNow; // đánh dấu token đã dùng

                await _users.UpdateAsync(user, innerCt);

                await _emailTokenRepository.UpdateTokenAsync(tokenInfo, innerCt);

                // (5) KHÔNG gọi _uow.SaveChangesAsync() ở đây!
                // ExecuteInTransactionAsync sẽ tự SaveChanges + Commit nếu không có exception.

                return ResponseConst.Success("Xác thực email thành công", true);

            }, IsolationLevel.ReadCommitted, ct);
        }

       
       
    }
}
