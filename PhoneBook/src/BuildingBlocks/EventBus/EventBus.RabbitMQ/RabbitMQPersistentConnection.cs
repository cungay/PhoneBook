using System.Net.Sockets;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {
        #region Fields

        private readonly IConnectionFactory connectionFactory = null;
        private readonly int retryCount = 0;
        private IConnection connection = null;
        private readonly object lock_object = new();
        private bool disposed = false;

        #endregion

        #region Properties

        public bool IsConnected => connection != null && connection.IsOpen;

        #endregion

        #region Ctor

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            this.connectionFactory = connectionFactory;
            this.retryCount = retryCount;
        }

        #endregion

        #region Methods

        public bool TryConnect()
        {
            lock (lock_object)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {

                    });

                policy.Execute(() =>
                {
                    connection = connectionFactory.CreateConnection();
                });

                if (IsConnected)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;

                    //log
                    return true;
                }

                return false;
            }
        }

        public IModel CreateModel()
        {
            return connection.CreateModel();
        }

        #endregion

        #region Events

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnCallbackException(object? sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnConnectionBlocked(object? sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            disposed = true;
            throw new NotImplementedException();
        }

        #endregion
    }
}
