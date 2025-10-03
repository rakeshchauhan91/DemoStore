using Infrastructure.Defaults;

namespace Infrastructure.Defaults.Services
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : IntegrationEvent;
    }
}
