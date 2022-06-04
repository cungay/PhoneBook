using System.Text;
using EventBus.Base;
using EventBus.Base.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        #region Fields

        private RabbitMQPersistentConnection persistentConnection = null;
        private readonly IConnectionFactory connectionFactory = null;
        private readonly IModel consumerChannel = null;

        #endregion

        #region Ctor

        public EventBusRabbitMQ(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            if (config.Connection != null)
            {
                var connJson = JsonConvert.SerializeObject(config, new JsonSerializerSettings()
                {
                    //because there are more than one recursive name with the same name, we add it to avoid errors
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

                connectionFactory = JsonConvert.DeserializeObject<IConnectionFactory>(connJson);
            }
            else
            {
                connectionFactory = new ConnectionFactory();
            }

            this.persistentConnection = new RabbitMQPersistentConnection(connectionFactory, config.ConnectionRetryCount);
            this.consumerChannel = CreateConsumerChannel();
            this.SubManager.OnRemoved += SubManagerOnRemoved;
        }

        #endregion

        #region Methods

        public override void Publish(IntegrationEvent @event)
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    //logging
                });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            //Ensure exchange exists while publishing
            consumerChannel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2; //persistent

                consumerChannel.QueueDeclare(
                    queue: GetSubName(eventName), //Ensure queue exists while publishing 
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                consumerChannel.BasicPublish(
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }

        public override void Subscribe<T, H>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!SubManager.HasSubscriptionsForEvent(eventName))
            {
                if (!persistentConnection.IsConnected)
                {
                    persistentConnection.TryConnect();
                }

                consumerChannel.QueueDeclare(
                    queue: GetSubName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                consumerChannel.QueueBind(queue: GetSubName(eventName),
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName);
            }

            SubManager.AddSubscription<T, H>();
            StartBasicConsume(eventName);
        }

        public override void Unsubscribe<T, H>()
        {
            SubManager.RemoveSubscription<T, H>();
        }

        #endregion

        #region Events

        private async void ConsumerOnReceived(object? sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            eventName = ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(e.Body.Span);
            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
            }
            consumerChannel.BasicAck(e.DeliveryTag, multiple: false);
        }

        private void SubManagerOnRemoved(object? sender, string eventName)
        {
            eventName = ProcessEventName(eventName);

            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }

            consumerChannel.QueueUnbind(
                queue: eventName,
                exchange: EventBusConfig.DefaultTopicName,
                routingKey: eventName);

            if (SubManager.IsEmpty)
            {
                consumerChannel.Close();
            }
        }

        #endregion

        #region Utility

        private IModel CreateConsumerChannel()
        {
            if (!persistentConnection.IsConnected)
            {
                persistentConnection.TryConnect();
            }
            var channel = persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");
            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if (consumerChannel != null)
            {
                var consumer = new EventingBasicConsumer(consumerChannel);
                consumer.Received += ConsumerOnReceived;
                consumerChannel.BasicConsume(queue: GetSubName(eventName), autoAck: false, consumer: consumer);
            }
        }

        #endregion
    }
}
