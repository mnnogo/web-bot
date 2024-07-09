using HealthMonitor.RabbitMQ;
using NLog;

namespace HealthMonitor
{
    internal class Program
    {
        private static readonly List<string> queues = ["pre-queue", "queue", "post-queue", "sending-queue"];
        private const string TEST_MESSAGE = "test message";
        private const string FLAG_PATH = "../../chatbot/webapp/stop.flag";

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("HealthMonitor is working...");

            foreach (var queue in queues)
            {
                Queue.StartListening(queue, (message) => HandleMessage(queue, message));
            }

            while (true)
            {
                try
                {
                    foreach (var queue in queues)
                    {
                        Queue.SendMessage(queue, TEST_MESSAGE, "HealthCheckTag");
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.ToString());

                    File.Create(FLAG_PATH).Dispose();
                }

                Thread.Sleep(10 * 1000);
            }
        }

        private static void HandleMessage(string queue, string? receivedMessage)
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
