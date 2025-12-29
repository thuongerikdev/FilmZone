using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using FZ.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FZ.Auth.Dtos.User.ProfileResponseDto;
using static FZ.Auth.Infrastructure.Repository.Implements.UserRepository;

namespace FZ.Auth.ApplicationService.MFAService.Abtracts
{
    public interface IAuthRegisterService
    {
        Task<ResponseDto<RegisterResponse>> RegisterAsync(RegisterRequest req, CancellationToken ct);
        Task<ResponseDto<bool>> VerifyEmailAsync(VerifyEmailRequest req, CancellationToken ct);
        Task<ResponseDto<RegisterResponse>> CreateSimpleUserAsync(SimpleCreateUserRequest req, CancellationToken ct);

    }
    public interface IPasswordService
    {
        Task<ResponseDto<ForgotPasswordResponse>> RequestResetAsync(ForgotPasswordRequest req, CancellationToken ct);
        Task<ResponseDto<VerifyResetCodeResponse>> VerifyResetCodeAsync(VerifyResetCodeRequest req, CancellationToken ct);
        Task<ResponseDto<bool>> ConfirmResetAsync(ResetPasswordConfirmRequest req, CancellationToken ct);
    }

    public interface IPasswordChangeService
    {
        // Email flow
        Task<ResponseDto<bool>> StartChangeByEmailAsync(string email, CancellationToken ct);
        Task<ResponseDto<string>> VerifyEmailCodeAsync(string email, string code, CancellationToken ct); // returns ticket

        // MFA flow
        Task<ResponseDto<string>> VerifyMfaCodeAsync(int userId, string code, CancellationToken ct); // returns ticket

        // Commit
        Task<ResponseDto<bool>> CommitChangeAsync(int userId, string ticket, string oldPwd, string newPwd, CancellationToken ct);



        // Forgot password (THÊM MỚI)
        Task<ResponseDto<bool>> StartForgotByEmailAsync(string email, CancellationToken ct);
        Task<ResponseDto<string>> VerifyForgotEmailCodeAsync(string email, string code, CancellationToken ct);     // ticket
        Task<ResponseDto<string>> VerifyForgotMfaCodeAsync(string email, string code, CancellationToken ct);       // ticket
        Task<ResponseDto<bool>> CommitForgotAsync(string ticket, string newPwd, CancellationToken ct);
    }


    public interface IAuthLoginService
    {
        Task<ResponseDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct);
        Task<ResponseDto<LoginResponse>> LoginWithGoogleAsync(AuthLoginGoogleRequest req, CancellationToken ct);
        Task<ResponseDto<LoginResponse>> VerifyMfaAndLoginAsync(MfaLoginVerifyRequest req, CancellationToken ct);

        Task<ResponseDto<bool>> LogoutByRefreshTokenAsync(string refreshToken, CancellationToken ct);
        Task<ResponseDto<bool>> LogoutSessionAsync(int userId, int sessionId, CancellationToken ct);
        Task<ResponseDto<int>> LogoutAllDevicesAsync(int userId, CancellationToken ct);

    }

    public interface IAuthUserService
    {
        Task<ResponseDto<List<UserSlimDto>>> GetAllSlimAsync(CancellationToken ct);
        Task<ResponseDto<AuthUser>> DeleteUserAsync(int userID, CancellationToken ct);
        Task<ResponseDto<GetUserResponseDto>> GetUserByIDAsync(int userID, CancellationToken ct);

        Task<ResponseDto<bool>> AuthUpdateProfileRequest ( AuthUpdateProfileRequest req, CancellationToken ct);
        Task<ResponseDto<bool>> AuthUpdateUserName(int userID, string userName, CancellationToken ct);
    }
}
