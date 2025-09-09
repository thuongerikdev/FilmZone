﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Dtos.User
{
    public class RegisterRequest
    {
        public string userName { get; set; }
        public string email { get; set; }
        public string password { get; set; }

        // tuỳ chọn kèm profile
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string gender { get; set; }
    }

    public class LoginRequest
    {
        public string userName { get; set; }
        public string password { get; set; }
    }

    public sealed class MfaLoginVerifyRequest { public string mfaTicket { get; set; } = default!; public string code { get; set; } = default!; }

    public sealed class LogoutRequest
    {
        // Truyền refresh token của thiết bị muốn đăng xuất.
        // (Nếu bạn để refresh token trong HttpOnly cookie thì đọc ở Controller và nhét vào đây)
        public string refreshToken { get; set; } = string.Empty;
    }


    




    public class AuthLoginGoogleRequest
    {
        public string? email { get; set; }
        public string? phoneNumber { get; set; }
        public string? fullName { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string? gender { get; set; }
        public string? avatar { get; set; }
        public string GoogleSub { get; set; } // Google user identifier
    }


    // Pashword reset
    public sealed class ForgotPasswordRequest
    {
        public string emailOrUserName { get; set; } = default!;
    }
    public sealed class VerifyResetCodeRequest
    {
        public string emailOrUserName { get; set; } = default!;
        public string token { get; set; } = default!;
    }
    public sealed class ResetPasswordConfirmRequest
    {
        public int userID { get; set; }
        public string resetTicket { get; set; } = default!;
        public string newPassword { get; set; } = default!;
    }

    //changePassword
    public sealed class StartChangeByEmailRequest
    {
        public string email { get; set; } = default!;
    }
    public sealed class VerifyChangeByEmailRequest
    {
        public string requestId { get; set; } = default!;
        public string email { get; set; } = default!;
        public string code { get; set; } = default!;
    }
    public sealed class CommitChangeByEmailRequest
    {
        public string requestId { get; set; } = default!;
        public string email { get; set; } = default!;
        public string oldPassword { get; set; } = default!;
        public string newPassword { get; set; } = default!;
    }





    public class VerifyEmailRequest
    {
        public int userID { get; set; }
        public string token { get; set; }
    }


}
