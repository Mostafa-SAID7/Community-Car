using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityCar.Domain.Interfaces.Common;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void ClearTracker();
}
