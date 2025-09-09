using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ.Auth.Infrastructure.Repository.Abtracts
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action,
                                             System.Data.IsolationLevel iso = System.Data.IsolationLevel.ReadCommitted,
                                             CancellationToken ct = default);
    }
}
