using EventBus.Base.Events;

namespace EventBus.Base.Abstraction
{
    public interface IIntegrationEventHandler<TEvent> : IntegrationEventHandler where TEvent : IntegrationEvent
    {
        Task Handle(TEvent @event);
    }

    public interface IntegrationEventHandler
    {

    }
}
