using HealthMonitor.RabbitMQ;
using NLog;

namespace HealthMonitor.Models
{
    public class HealthMonitorTask : BackgroundService
    {
        private readonly List<string> queues = ["pre-queue", "queue", "post-queue"];
        private const string TEST_MESSAGE = "test message";
        private const string FLAG_PATH = "../../chatbot/webapp/stop.flag";

        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var queue in queues)
                    {
                        Queue.SendMessage(queue, TEST_MESSAGE, "HealthCheckTag");
                        Queue.StartListening(queue, (message) => HandleMessage(queue, message));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());

                    File.Create(FLAG_PATH).Dispose();
                }

                Thread.Sleep(30 * 1000);
            }

            return Task.CompletedTask;
        }

        private void HandleMessage(string queue, string? receivedMessage)
        {
            if (receivedMessage == TEST_MESSAGE)
            {
                DeleteFlagFileIfExists(FLAG_PATH);

                _logger.Info("Queue {queue} is working correctly", queue);
            }
            else
            {
                _logger.Error("Queue {queue} is NOT working correctly", queue);
            }
        }

        private static void DeleteFlagFileIfExists(string filepath)
        {
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }
    }
}