using Microsoft.Extensions.DependencyInjection;
using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        #region Properties

        public readonly IServiceProvider ServiceProvider = null;
        public readonly IEventBusSubscriptionManager SubManager = null;
        public EventBusConfig EventBusConfig = null;

        #endregion

        #region Ctor

        public BaseEventBus(EventBusConfig config, IServiceProvider serviceProvider)
        {
            this.EventBusConfig = config;
            this.ServiceProvider = serviceProvider;
            this.SubManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }

        #endregion

        #region Methods

        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
                eventName = eventName.TrimStart(EventBusConfig.EventNamePrefix.ToArray());

            if (EventBusConfig.DeleteEventSuffix)
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);

            var processed = false;

            if (SubManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = SubManager.GetHandlersForEvent(eventName);

                using (var scope = ServiceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandlerType);
                        if (handler is null) continue;

                        var eventType = SubManager.GetEventTypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }
                processed = true;
            }

            return processed;
        }

        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, H>() where T : IntegrationEvent where H : IIntegrationEventHandler<T>;

        public abstract void Unsubscribe<T, H>() where T : IntegrationEvent where H : IIntegrationEventHandler<T>;

        #endregion

        #region Dispose

        public virtual void Dispose()
        {
            EventBusConfig = null;
            SubManager.Clear();
        }

        #endregion
    }
}
