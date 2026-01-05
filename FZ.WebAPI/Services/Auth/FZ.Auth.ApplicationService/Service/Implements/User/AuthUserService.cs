using FZ.Auth.ApplicationService.MFAService.Abtracts;
using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using FZ.Auth.Infrastructure.Repository.Abtracts; // <- dùng repo
using FZ.Constant;
using Microsoft.Extensions.Logging;

namespace FZ.Auth.ApplicationService.MFAService.Implements.User
{
    public class AuthUserService : IAuthUserService
    {
        private readonly ILogger<AuthUserService> _logger;
        private readonly IUserRepository _users;   // <-- inject repo, không phải IAuthUserService
        private readonly IUnitOfWork _uow;

        public AuthUserService(ILogger<AuthUserService> logger, IUserRepository users , IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _users = users;
            _uow = unitOfWork;
        }

        public async Task<ResponseDto<List<UserSlimDto>>> GetAllSlimAsync(CancellationToken ct)
        {
            _logger.LogInformation("Fetching all users in slim mode.");
            try
            {
                var list = await _users.GetAllSlimAsync(ct);
                _logger.LogInformation("Successfully fetched all users in slim mode.");
                return ResponseConst.Success("Fetched all users in slim mode successfully.", list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all users in slim mode.");
                return ResponseConst.Error<List<UserSlimDto>>(500, "Internal error");
            }
        }
        public async Task<ResponseDto<AuthUser>> DeleteUserAsync(int userID, CancellationToken ct)
        {
            _logger.LogInformation("Deleting user with ID: {UserID}", userID);
            try
            {
                var user = await _users.DeleteUser(userID, ct);
                await _uow.SaveChangesAsync(ct);


                _logger.LogInformation("Successfully deleted user with ID: {UserID}", userID);
                return ResponseConst.Success("User deleted successfully", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID: {UserID}", userID);
                return ResponseConst.Error<AuthUser>(500, "Internal error");
            }
        }

    }
}
