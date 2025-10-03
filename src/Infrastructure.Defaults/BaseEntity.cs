 

namespace Infrastructure.Defaults
{
    public interface IEntity<T>
    {
        T Id { get; set; }
    }

    // Base Entity Class
    public abstract class BaseEntity<T> : IEntity<T>
    {
        public T Id { get; set; }
        public Guid ExternalId { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastSyncDt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = false;
    }
}
