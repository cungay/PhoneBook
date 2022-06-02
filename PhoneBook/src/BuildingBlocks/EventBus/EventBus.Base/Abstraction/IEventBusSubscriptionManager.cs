using EventBus.Base.Events;

namespace EventBus.Base.Abstraction
{
    public interface IEventBusSubscriptionManager
    {
        event EventHandler<string> OnRemoved;

        bool IsEmpty { get; }

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;

        bool HasSubscriptionsForEvent(string eventName);

        string GetEventKey<T>();

        Type GetEventTypeByName(string eventName);

        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        void AddSubscription<T, H>() where T : IntegrationEvent where H : IIntegrationEventHandler<T>;

        void RemoveSubscription<T, H>() where H : IIntegrationEventHandler<T> where T : IntegrationEvent;

        void Clear();
    }
}
