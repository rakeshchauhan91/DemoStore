 
using System.Linq.Expressions;
 
namespace Infrastructure.Defaults
{
    public class SearchCriteria<T>
    {
        public Expression<Func<T, bool>>? Filter { get; set; }
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; set; }
        public string? IncludeProperties { get; set; }
        public bool AsNoTracking { get; set; } = false;
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
