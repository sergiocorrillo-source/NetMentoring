using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ticketing.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<Task> operation);
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    }
}
