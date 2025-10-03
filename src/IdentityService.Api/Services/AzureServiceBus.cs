using Azure.Messaging.ServiceBus;
using Infrastructure.Defaults;
using Newtonsoft.Json;

namespace IdentityService.Api.Services
{

    // Mock implementation for MVP. In production, this would use a real broker client.
    public class MockEventBus : IEventPublisher
    {
        private readonly ILogger<MockEventBus> _logger;

        public MockEventBus(ILogger<MockEventBus> logger)
        {
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IntegrationEvent
        {
            _logger.LogInformation("--- Publishing Event: {EventName} ({EventId})",
                @event.GetType().Name, @event.Id);

            // In a real microservice, this would serialize the event and push to a queue/topic
            // Example: _rabbitMqClient.Publish(@event);
             
        }
    }
    public class AzureServiceBusEventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureServiceBusEventPublisher> _logger;

        public AzureServiceBusEventPublisher(IConfiguration configuration, ILogger<AzureServiceBusEventPublisher> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var connectionString = _configuration.GetConnectionString("AzureServiceBus");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Azure Service Bus connection string is not configured.");
                throw new InvalidOperationException("Azure Service Bus connection string is not configured.");
            }
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        public async Task PublishAsync<T>(T @event ) where T : IntegrationEvent
        {
            var eventName = @event.GetType().Name; // e.g., "UserRegisteredEvent"
            var topicName = _configuration[$"AzureServiceBus:Topics:{eventName}"];

            if (string.IsNullOrEmpty(topicName))
            {
                _logger.LogWarning($"No Azure Service Bus topic configured for event '{eventName}'. Event will not be published.");
                return;
            }

            ServiceBusSender sender = _serviceBusClient.CreateSender(topicName);

            try
            {
                var jsonMessage = JsonConvert.SerializeObject(@event);
                var message = new ServiceBusMessage(jsonMessage)
                {
                    ContentType = "application/json",
                    Subject = eventName,
                    MessageId = Guid.NewGuid().ToString() // Unique ID for traceability
                };

                await sender.SendMessageAsync(message);
                _logger.LogInformation($"Event '{eventName}' published to topic '{topicName}' with MessageId: {message.MessageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing event '{eventName}' to topic '{topicName}'.");
                // Depending on requirements, you might re-throw, log and suppress, or use a dead-letter queue pattern.
                throw;
            }
            finally
            {
                await sender.DisposeAsync();
            }
        }
    }
}
