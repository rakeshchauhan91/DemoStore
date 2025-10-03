 
namespace Infrastructure.Defaults
{
    public class PaginationRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortField { get; set; }
        public bool SortDescending { get; set; } = false;
        public PaginationRequest(int page, int pageSize)
        {   
            Page = page;    
            PageSize = pageSize;
                
        }
    }
}
