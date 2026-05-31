using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ticketing.Data;

namespace Ticketing.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TicketingDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private bool _disposed;

        public UnitOfWork(TicketingDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repo = new Repository<T>(_context);
                _repositories[type] = repo;
            }
            return (IRepository<T>)_repositories[type]!;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            // InMemory provider does not support real transactions. Detect and fallback.
            var databaseProvider = _context.Database.ProviderName;
            if (databaseProvider != null && databaseProvider.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                // InMemory provider doesn't support transactions. Execute the operation and on exception
                // attempt a simple rollback by deleting the in-memory database so state returns to clean.
                try
                {
                    await operation().ConfigureAwait(false);
                    return;
                }
                catch
                {
                    try
                    {
                        await _context.Database.EnsureDeletedAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignore cleanup failures
                    }
                    throw;
                }
            }

            using var tx = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                await operation().ConfigureAwait(false);
                await tx.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await tx.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}
