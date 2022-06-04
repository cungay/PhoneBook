using EventBus.Base.Abstraction;
using EventBus.UnitTest.Events;

namespace EventBus.UnitTest.EventHandlers
{
    public class ContactCreatedEventHandler : IIntegrationEventHandler<ContactCreatedEvent>
    {
        public Task Handle(ContactCreatedEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
