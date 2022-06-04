using EventBus.Base.Events;

namespace EventBus.UnitTest.Events
{
    public class ContactCreatedEvent : IntegrationEvent
    {
        public int Id { get; set; }

        public ContactCreatedEvent(int id)
        {
            Id = id;
        }
    }
}
