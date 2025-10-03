using System.Data.Common;
using System.Linq.Expressions;

namespace Infrastructure.Defaults.Persistence
{
    public interface IReadOnlyRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(TKey id, string includeProperties, CancellationToken cancellationToken = default);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, string includeProperties, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(string includeProperties, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindAsync(SearchCriteria<TEntity> criteria, CancellationToken cancellationToken = default);
    }

    public interface IPaginatedRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        Task<PaginatedResult<TEntity>> GetPaginatedAsync(PaginationRequest request, CancellationToken cancellationToken = default);
        Task<PaginatedResult<TEntity>> GetPaginatedAsync(PaginationRequest request, Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken = default);
        Task<PaginatedResult<TEntity>> GetPaginatedAsync(PaginationRequest request, SearchCriteria<TEntity> criteria, CancellationToken cancellationToken = default);
    }

    public interface ICountableRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    }

    // --- Core Write Operations ---

    public interface IWriteRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a full generic repository with common CRUD, pagination, and counting.
    /// </summary>
    public interface IGenericRepository<TEntity, TKey> :
        IReadRepository<TEntity, TKey>,
        IWriteRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        Task<IEnumerable<TEntity>> FromSqlRawAsync(string sql, params object[] parameters);

        Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params DbParameter[] parameters) where T : class; // Use SqlParameter
        Task<int> ExecuteNonQueryStoredProcedureAsync(string procedureName, params DbParameter[] parameters); // Use SqlParameter
    }

    /// <summary>
    /// Combines all read operations for a repository.
    /// </summary>
    public interface IReadRepository<TEntity, TKey> :
        IReadOnlyRepository<TEntity, TKey>,
        IPaginatedRepository<TEntity, TKey>,
        ICountableRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    { }
}
