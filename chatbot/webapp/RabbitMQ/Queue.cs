using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using NLog;

namespace webapp.RabbitMQ
{
    public class Queue
    {
        private static readonly ConnectionFactory _factory;
        private static readonly IConnection _connection;
        private static readonly IModel _channel;
        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        static Queue()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public static void SendMessage(string queueName, string message)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);

            _logger.Debug($"Message '{message}' sent to '{queueName}'");
        }

        public static void StartListening(string queueName, Action<string> onMessageReceived)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.Headers != null &&
                    ea.BasicProperties.Headers.ContainsKey("MonitorTag"))
                {
                    // игнорируем сообщение
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                    return;
                }

                _logger.Debug("Message {message} was consumed from {queueName}", message, queueName);

                _channel.BasicAck(ea.DeliveryTag, true);

                onMessageReceived(message);
            };

            _channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        public static void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
