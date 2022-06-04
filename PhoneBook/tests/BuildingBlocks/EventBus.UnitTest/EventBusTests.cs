using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void subscribe_event_on_rabbitmq_test()
        {
            Assert.Pass();
        }
    }
}