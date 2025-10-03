using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;

namespace Infrastructure.Defaults.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IEntity<TKey>;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params DbParameter[] parameters) where T : class;
        Task<int> ExecuteNonQueryStoredProcedureAsync(string procedureName, params DbParameter[] parameters);
    }
    public class UnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
    {
        private readonly TDbContext _context;
        // Cache to store instantiated repositories to ensure one instance per UoW
        private readonly ConcurrentDictionary<Type, object> _repositories;

        private IDbContextTransaction? _transaction;

        public UnitOfWork(TDbContext context)
        {
            _context = context;
            _repositories = new ConcurrentDictionary<Type, object>();
        }
   
       // This method instantiates the GenericRepository with the specific context
        public IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class, IEntity<TKey>
        {
            var entityType = typeof(TEntity);

            // Check cache first
            if (_repositories.TryGetValue(entityType, out var repository))
            {
                return (IGenericRepository<TEntity, TKey>)repository;
            }

            // Dynamically create the GenericRepository<TEntity, TKey, TDbContext>
            var repositoryType = typeof(GenericRepository<,,>).MakeGenericType(entityType, typeof(TKey), typeof(TDbContext));

            // TDbContext is passed to the constructor (GenericRepository(DbContext context))
            repository = Activator.CreateInstance(repositoryType, _context)
                ?? throw new InvalidOperationException($"Could not create instance of {repositoryType.Name}");

            _repositories.TryAdd(entityType, repository);
            return (IGenericRepository<TEntity, TKey>)repository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _transaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = $"@p{i}";
                        parameter.Value = parameters[i] ?? DBNull.Value;
                        command.Parameters.Add(parameter);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params DbParameter[] parameters) where T : class
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using var reader = await command.ExecuteReaderAsync();
                var results = new List<T>();

                // Note: This is a simplified implementation
                // You'll need to implement proper mapping from DbDataReader to T
                // Consider using a mapping library like Dapper for this

                return results;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<int> ExecuteNonQueryStoredProcedureAsync(string procedureName, params DbParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                return await command.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
