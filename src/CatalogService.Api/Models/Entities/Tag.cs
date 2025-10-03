


using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Api.Models.Entities
{
    public class Tag : BaseEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}