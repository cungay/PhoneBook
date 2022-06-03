using EventBus.Base;
using EventBus.Base.Events;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
        }

        public override void Publish(IntegrationEvent @event)
        {
            throw new NotImplementedException();
        }

        public override void Subscribe<T, H>()
        {
            throw new NotImplementedException();
        }

        public override void Unsubscribe<T, H>()
        {
            throw new NotImplementedException();
        }
    }
}
