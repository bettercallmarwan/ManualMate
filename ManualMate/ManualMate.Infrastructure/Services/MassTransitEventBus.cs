using ManualMate.Application.Interfaces.Services;
using MassTransit;

namespace ManualMate.Infrastructure.Services;

public class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IIntegrationEventBus
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent)
        => publishEndpoint.Publish(integrationEvent!);
}