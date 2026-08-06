namespace ManualMate.Application.Interfaces.Services;

public interface IIntegrationEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent);
}