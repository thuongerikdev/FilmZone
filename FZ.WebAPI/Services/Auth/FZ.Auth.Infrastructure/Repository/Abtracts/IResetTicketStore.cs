using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IResetTicketStore
    {
        Task<string> IssueAsync(int userId, TimeSpan ttl, CancellationToken ct);
        Task<bool> ConsumeAsync(int userId, string ticket, CancellationToken ct);
    }
}
