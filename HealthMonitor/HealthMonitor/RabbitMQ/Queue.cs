using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using NLog;

namespace HealthMonitor.RabbitMQ
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

        public static void SendMessage(string queueName, string message, string tag)
        {
            _channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var properties = _channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object> { { "MonitorTag", tag } };

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: body);

            _logger.Debug("Message {message} sent to {queueName}", message, queueName);
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

                if (ea.BasicProperties.Headers == null ||
                    !ea.BasicProperties.Headers.ContainsKey("MonitorTag"))
                {
                    // игнорируем сообщение
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                    return;
                }

                _channel.BasicAck(ea.DeliveryTag, false);
                _logger.Debug("Message {message} was consumed from {queueName}", message, queueName);

                onMessageReceived(message);
            };

            _channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
        }

        public static string? ReceiveMessage(string queueName, string expectedTag)
        {
            var result = _channel.BasicGet(queue: queueName, autoAck: false);
            if (result != null)
            {              
                var message = Encoding.UTF8.GetString(result.Body.ToArray());

                // является ли сообщение тест-сообщением с тегом
                if (result.BasicProperties.Headers != null &&
                    result.BasicProperties.Headers.TryGetValue("MonitorTag", out object? value) &&
                    Encoding.UTF8.GetString((byte[])value) == expectedTag)
                {
                    _channel.BasicAck(result.DeliveryTag, false);
                    return message;
                }
            }
            return null;
        }

        public static void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
