using FZ.Auth.Domain.MFA;
using FZ.Auth.Dtos;
using FZ.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.MFAService.Abtracts
{
    public interface IMfaService
    {
        Task<ResponseDto<StartTotpResponse>> StartTotpEnrollmentAsync(int userId, string? label, CancellationToken ct);
        Task<ResponseDto<bool>> ConfirmTotpEnrollmentAsync(int userId, string code, CancellationToken ct);
        Task<ResponseDto<bool>> DisableTotpAsync(int userId, string? confirmCode, CancellationToken ct);
        Task<bool> VerifyTotpAsync(int userId, string code, CancellationToken ct);

        Task<ResponseDto<AuthMfaSecret>> GetByUserAsync(int userId, CancellationToken ct);
        Task<ResponseDto<AuthMfaSecret>> GetByIdAsync(int id, CancellationToken ct);
        Task<ResponseDto<List<AuthMfaSecret>>> GetAllMFAAsync(CancellationToken ct);
    }
}
