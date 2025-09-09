﻿// ApplicationService/Service/Implements/Account/MfaService.cs
using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.Domain.MFA;
using FZ.Auth.Dtos;
using FZ.Auth.Infrastructure.Repository.Abtracts;
using FZ.Constant;
using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.MFAService.Implements.Account
{
    public sealed class MfaService : IMfaService
    {
        private readonly ILogger<MfaService> _logger;
        private readonly IMFARepository _repo;
        private readonly IUnitOfWork _uow;

        public MfaService(ILogger<MfaService> logger, IMFARepository repo, IUnitOfWork uow)
        {
            _logger = logger;
            _repo = repo;
            _uow = uow;
        }

        public async Task<ResponseDto<StartTotpResponse>> StartTotpEnrollmentAsync(int userId, string? label, CancellationToken ct)
        {
            try
            {
                // tạo secret mới (20 bytes)
                var key = KeyGeneration.GenerateRandomKey(20);
                var base32 = Base32Encoding.ToString(key);

                // issuer/label hiển thị trên app
                var issuer = "FZ Movies";
                var account = label ?? $"user:{userId}";
                var otpauth = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}?secret={base32}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30&algorithm=SHA1";

                var entity = await _repo.GetByUserAsync(userId, ct) ?? new AuthMfaSecret { userID = userId, type = "TOTP" };
                entity.secret = base32;
                entity.label = account;
                entity.status = "Pending";
                entity.isEnabled = false;
                entity.enrollmentStartedAt = DateTime.UtcNow;
                entity.updatedAt = DateTime.UtcNow;

                await _repo.UpsertAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                return ResponseConst.Success("Đã cấp secret/QR để đăng ký MFA.", new StartTotpResponse
                {
                    secretBase32 = base32,
                    otpauthUri = otpauth,
                    label = account
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StartTotpEnrollmentAsync error");
                return ResponseConst.Error<StartTotpResponse>(500, "Không khởi tạo được MFA");
            }
        }

        public async Task<ResponseDto<bool>> ConfirmTotpEnrollmentAsync(int userId, string code, CancellationToken ct)
        {
            try
            {
                var entity = await _repo.GetByUserAsync(userId, ct);
                if (entity == null || entity.status != "Pending" || string.IsNullOrWhiteSpace(entity.secret))
                    return ResponseConst.Error<bool>(400, "MFA chưa được khởi tạo hoặc secret không hợp lệ.");

                if (!VerifyCode(entity.secret, code))
                    return ResponseConst.Error<bool>(400, "Mã xác thực không đúng.");

                entity.status = "Enabled";
                entity.isEnabled = true;
                entity.enabledAt = DateTime.UtcNow;
                entity.lastVerifiedAt = DateTime.UtcNow;
                entity.updatedAt = DateTime.UtcNow;

                await _repo.UpsertAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                return ResponseConst.Success("Đã bật MFA (TOTP).", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmTotpEnrollmentAsync error");
                return ResponseConst.Error<bool>(500, "Không bật được MFA");
            }
        }

        public async Task<ResponseDto<bool>> DisableTotpAsync(int userId, string? confirmCode, CancellationToken ct)
        {
            try
            {
                var entity = await _repo.GetByUserAsync(userId, ct);
                if (entity == null || entity.status != "Enabled")
                    return ResponseConst.Success("MFA đã tắt hoặc chưa bật.", true);

                // tuỳ chính sách: yêu cầu nhập code để tắt
                if (!string.IsNullOrWhiteSpace(confirmCode))
                {
                    if (string.IsNullOrWhiteSpace(entity.secret) || !VerifyCode(entity.secret, confirmCode))
                        return ResponseConst.Error<bool>(400, "Mã xác thực không đúng.");
                }

                entity.status = "Disabled";
                entity.isEnabled = false;
                entity.updatedAt = DateTime.UtcNow;

                await _repo.UpsertAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);

                return ResponseConst.Success("Đã tắt MFA.", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DisableTotpAsync error");
                return ResponseConst.Error<bool>(500, "Không tắt được MFA");
            }
        }

        public async Task<bool> VerifyTotpAsync(int userId, string code, CancellationToken ct)
        {
            var entity = await _repo.GetByUserAsync(userId, ct);
            if (entity == null || entity.status != "Enabled" || string.IsNullOrWhiteSpace(entity.secret))
                return false;

            var ok = VerifyCode(entity.secret, code);
            if (ok)
            {
                entity.lastVerifiedAt = DateTime.UtcNow;
                entity.updatedAt = DateTime.UtcNow;
                await _repo.UpsertAsync(entity, ct);
                await _uow.SaveChangesAsync(ct);
            }
            return ok;
        }

        private static bool VerifyCode(string base32Secret, string code)
        {
            try
            {
                var bytes = Base32Encoding.ToBytes(base32Secret);
                var totp = new Totp(bytes, step: 30, mode: OtpHashMode.Sha1, totpSize: 6);
                // chấp nhận lệch 1 step để đỡ fail
                return totp.VerifyTotp(code.Trim(), out _, window: new VerificationWindow(previous: 1, future: 1));
            }
            catch { return false; }
        }
    }
}
