using Domain.Base;

namespace Application.Interfaces.UOW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : class;
        Task SaveAsync();
        void Save();
        void BeginTransaction();
        Task BeginTransactionAsync();
        void CommitTransaction();
        Task CommitTransactionAsync();
        void RollBack();
        Task<bool> IsValidAsync<T>(string id) where T : BaseEntity;
    }
}
