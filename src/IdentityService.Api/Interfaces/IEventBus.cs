using Infrastructure.Defaults;

namespace IdentityService.Api.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : IntegrationEvent;
    }
}
