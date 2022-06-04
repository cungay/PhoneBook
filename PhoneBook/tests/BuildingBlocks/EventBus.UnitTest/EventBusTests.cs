using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Events;
using EventBus.UnitTest.EventHandlers;

namespace EventBus.UnitTest
{
    public class EventBusTests
    {
        private ServiceCollection services = null;

        public EventBusTests()
        {
            services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
        }

        [Test]
        public void subscribe_event_on_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(sp =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount = 5,
                    SubscriberClientAppName = "EventBus.UnitTest",
                    DefaultTopicName = "PhoneBookTopic",
                    EventBusType = EventBusType.RabbitMQ,
                    EventNameSuffix = "IntegrationEvent",
                    //Connection = new ConnectionFactory()
                    //{
                    //    HostName = "localhost",
                    //    Port = 5672,
                    //    UserName = "guest",
                    //    Password = "guest"
                    //}
                };

                return EventBusFactory.Create(config, sp);
            });

            var sp = services.BuildServiceProvider();

            var eventBus = sp.GetRequiredService<IEventBus>();
            
            eventBus.Subscribe<ContactCreatedEvent, ContactCreatedEventHandler>();
            
            eventBus.Unsubscribe<ContactCreatedEvent, ContactCreatedEventHandler>();
        }
    }
}