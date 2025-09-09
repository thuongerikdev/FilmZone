using FZ.Auth.Domain.User;
using FZ.Auth.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IUserRepository
    {

        Task<AuthUser?> Login (string username, string password , CancellationToken ct);

        Task<List<UserSlimDto>> GetAllSlimAsync(CancellationToken ct);

        Task<AuthUser?> FindByGoogleSub(string googleSub, CancellationToken ct);

        Task<AuthUser> DeleteUser(int id, CancellationToken ct);

        Task<AuthUser?> FindByIdAsync(int id, CancellationToken ct);
        Task<AuthUser?> FindByUserNameAsync(string userName, CancellationToken ct);
        Task<AuthUser?> FindByEmailAsync(string email, CancellationToken ct);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct);

        Task UpdateAsync(AuthUser user, CancellationToken ct);

        //Task<bool> verifyEmail(VerifyEmailDto verifyEmailDto, CancellationToken ct);

        Task AddAsync(AuthUser user, CancellationToken ct);
    }
    public interface IProfileRepository
    {
        Task AddAsync(AuthProfile profile, CancellationToken ct);
        Task<AuthProfile?> GetByUserIdAsync(int userId, CancellationToken ct);
        Task<bool> ExistsByUserIdAsync(int userId, CancellationToken ct);
        Task UpdateAsync(AuthProfile profile, CancellationToken ct);
    }




    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }
    //public interface ITokenGenerator
    //{
    //    string GenerateToken(int size = 32);
    //}



}
