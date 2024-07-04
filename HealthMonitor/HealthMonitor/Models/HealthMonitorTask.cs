using HealthMonitor.RabbitMQ;
using NLog;

namespace HealthMonitor.Models
{
    public class HealthMonitorTask : BackgroundService
    {
        private readonly List<string> queues = ["pre-queue", "queue", "post-queue"];
        private const string TEST_MESSAGE = "test message";

        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var queue in queues)
                {
                    Queue.SendMessage(queue, TEST_MESSAGE, "HealthCheckTag");
                    //var receivedMessage = Queue.ReceiveMessage(queue, "HealthCheckTag");
                    Queue.StartListening(queue, (message) => HandleMessage(queue, message));
                }

                Thread.Sleep(60 * 1000);
            }

            return Task.CompletedTask;
        }

        private void HandleMessage(string queue, string? receivedMessage)
        {
            if (receivedMessage != TEST_MESSAGE)
            {
                _logger.Error("Queue {queue} is NOT working correctly", queue);
                //SendServerError();
                //throw new Exception();
            }
            else
            {
                _logger.Info("Queue {queue} is working correctly", queue);
            }
        }

        private void SendServerError()
        {
            throw new NotImplementedException();
        }
    }
}