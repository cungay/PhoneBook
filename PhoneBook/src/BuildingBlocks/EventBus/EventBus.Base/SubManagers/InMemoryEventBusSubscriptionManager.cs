using EventBus.Base.Abstraction;
using EventBus.Base.Events;

namespace EventBus.Base.SubManagers
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> handlers;
        private readonly List<Type> eventTypes;

        public event EventHandler<string> OnRemoved;
        public Func<string, string> eventNameGetter;

        #region Ctor

        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            this.handlers = new Dictionary<string, List<SubscriptionInfo>>();
            this.eventTypes = new List<Type>();
            this.eventNameGetter = eventNameGetter;
        }

        #endregion

        #region Properties

        public bool IsEmpty => !handlers.Keys.Any();

        #endregion

        #region Methods

        public void Clear() => handlers.Clear();

        public void AddSubscription<T, H>()
            where T : IntegrationEvent
            where H : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            AddSubscription(typeof(H), eventName);

            if (eventTypes.Contains(typeof(T)))
            {
                eventTypes.Add(typeof(T));
            }
        }

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName);
        }

        public Type GetEventTypeByName(string eventName)
        {
            return eventTypes.SingleOrDefault(t => t.Name == eventName);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => handlers[eventName];

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName) => handlers.ContainsKey(eventName);

        public void RemoveSubscription<T, H>()
            where T : IntegrationEvent
            where H : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, H>();
            var eventName = GetEventKey<T>();
            RemoveHandler(eventName, handlerToRemove);
        }

        #endregion

        #region Utilities

        private void AddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (handlers[eventName].Any(p => p.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
        }

        private void RemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                handlers[eventName].Remove(subsToRemove);

                if (handlers[eventName].Any())
                {
                    handlers.Remove(eventName);

                    var eventType = eventTypes.SingleOrDefault(e => e.Name == eventName);

                    if (eventType != null)
                    {
                        eventTypes.Remove(eventType);
                    }

                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, H>()
            where T : IntegrationEvent
            where H : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return FindSubscriptionToRemove(eventName, typeof(H));
        }

        private SubscriptionInfo FindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
                return null;
            return handlers[eventName].SingleOrDefault(e => e.HandlerType == handlerType);
        }

        #endregion
    }
}
