using Application.Interfaces.UOW;
using Domain.Base;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction; // Store transaction instance
        private bool disposed = false;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            return new GenericRepository<T>(_context);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
            _transaction = null;
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void RollBack()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public async Task<bool> IsValidAsync<T>(string id) where T : BaseEntity
        {
            var entity = await GetRepository<T>().GetByIdAsync(id);
            return entity is not null && entity.DeletedBy is null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _transaction?.Rollback();
                    _transaction?.Dispose();
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
