using EventBus.Base.Events;

namespace EventBus.Base.Abstraction
{
    public interface IEventBus : IDisposable
    {
        void Publish(IntegrationEvent @event);

        void Subscribe<T, H>() where T : IntegrationEvent where H : IIntegrationEventHandler<T>;

        void Unsubscribe<T, H>() where T : IntegrationEvent where H : IIntegrationEventHandler<T>;
    }
}
