using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq.Expressions;

namespace Infrastructure.Defaults.Persistence
{
    public abstract class GenericRepository<TEntity, TKey, TDbContext> : IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
        where TDbContext : DbContext
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, string includeProperties, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(filter, cancellationToken);
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, string includeProperties, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            return await query.FirstOrDefaultAsync(filter, cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(string includeProperties, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty.Trim());
            }

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(filter).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(SearchCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            if (criteria.AsNoTracking)
                query = query.AsNoTracking();

            if (criteria.Filter != null)
                query = query.Where(criteria.Filter);

            if (!string.IsNullOrEmpty(criteria.IncludeProperties))
            {
                foreach (var includeProperty in criteria.IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            if (criteria.OrderBy != null)
                query = criteria.OrderBy(query);

            if (criteria.Skip.HasValue)
                query = query.Skip(criteria.Skip.Value);

            if (criteria.Take.HasValue)
                query = query.Take(criteria.Take.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<PaginatedResult<TEntity>> GetPaginatedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
        {
            return await GetPaginatedAsync(request, filter: null, cancellationToken);
        }

        public virtual async Task<PaginatedResult<TEntity>> GetPaginatedAsync(PaginationRequest request, Expression<Func<TEntity, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            var totalCount = await query.CountAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.SortField))
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var property = Expression.Property(parameter, request.SortField);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = request.SortDescending ? "OrderByDescending" : "OrderBy";
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(TEntity), property.Type },
                    query.Expression,
                    lambda);

                query = query.Provider.CreateQuery<TEntity>(resultExpression);
            }

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<TEntity>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public virtual async Task<PaginatedResult<TEntity>> GetPaginatedAsync(PaginationRequest request, SearchCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            if (criteria.AsNoTracking)
                query = query.AsNoTracking();

            if (criteria.Filter != null)
                query = query.Where(criteria.Filter);

            if (!string.IsNullOrEmpty(criteria.IncludeProperties))
            {
                foreach (var includeProperty in criteria.IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (criteria.OrderBy != null)
                query = criteria.OrderBy(query);
            else if (!string.IsNullOrEmpty(request.SortField))
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var property = Expression.Property(parameter, request.SortField);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = request.SortDescending ? "OrderByDescending" : "OrderBy";
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(TEntity), property.Type },
                    query.Expression,
                    lambda);

                query = query.Provider.CreateQuery<TEntity>(resultExpression);
            }

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<TEntity>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(filter, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(e => e.Id!.Equals(id), cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(filter, cancellationToken);
        }
        private bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        private void SetPropertyValue(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            property?.SetValue(obj, value);
        }
        public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is BaseEntity<TKey> baseEntity)
            {
                baseEntity.CreatedAt = DateTime.UtcNow;
            }
            else if (HasProperty(entity, "CreatedAt"))
            {
                SetPropertyValue(entity, "CreatedAt", DateTime.UtcNow);
            }

            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }

        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var entitiesList = entities.ToList();

            foreach (var entity in entitiesList.OfType<BaseEntity<TKey>>())
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            await _dbSet.AddRangeAsync(entitiesList, cancellationToken);
            return entitiesList;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity is BaseEntity<TKey> baseEntity)
            {
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }

            _dbSet.Update(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var entitiesList = entities.ToList();

            foreach (var entity in entitiesList.OfType<BaseEntity<TKey>>())
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }

            _dbSet.UpdateRange(entitiesList);
            return await Task.FromResult(entitiesList);
        }

        public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

        public virtual async Task SoftDeleteAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity is BaseEntity<TKey> baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(entity, cancellationToken);
            }
        }

        public virtual async Task<IEnumerable<TEntity>> FromSqlRawAsync(string sql, params object[] parameters)
        {
            return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string procedureName, params DbParameter[] parameters) where T : class
        {
            var sql = $"EXEC {procedureName}";
            if (parameters.Length > 0)
            {
                var paramNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                sql += $" {paramNames}";
            }

            return await _context.Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }

        public virtual async Task<int> ExecuteNonQueryStoredProcedureAsync(string procedureName, params DbParameter[] parameters)
        {
            var sql = $"EXEC {procedureName}";
            if (parameters.Length > 0)
            {
                var paramNames = string.Join(", ", parameters.Select(p => p.ParameterName));
                sql += $" {paramNames}";
            }

            return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}
