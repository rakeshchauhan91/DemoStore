﻿ 
namespace Infrastructure.Defaults
{
    public abstract class IntegrationEvent
    {
        public Guid Id { get; }
        public DateTime CreationDate { get; }

        protected IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }
    }
}
