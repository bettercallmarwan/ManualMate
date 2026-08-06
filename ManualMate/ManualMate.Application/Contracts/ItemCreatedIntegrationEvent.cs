namespace ManualMate.Application.Contracts;

public record ItemCreatedIntegrationEvent
{
    public Guid ItemId { get; init; }
}