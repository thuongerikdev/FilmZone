using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.ApplicationService.MFAService.Abtracts
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        public Task TestSend(string to);
        //public Task<ResponeDto> SendInvoiceEmail(int userID);
        public Task SendVerificationEmail(int userId, string email, string verificationToken);
        Task SendPasswordResetEmail(int userId, string email, string resetToken);


    }
}
