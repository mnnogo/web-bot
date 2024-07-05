using HealthMonitor.Hubs;
using HealthMonitor.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace HealthMonitor.Models
{
    public class HealthMonitorTask : BackgroundService
    {
        private readonly List<string> queues = ["pre-queue", "queue", "post-queue"];
        private const string TEST_MESSAGE = "test message";

        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private IHubContext<MessageHub> _hubContext;

        public HealthMonitorTask(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var queue in queues)
                    {
                        Queue.SendMessage(queue, TEST_MESSAGE, "HealthCheckTag");
                        Queue.StartListening(queue, (message) => HandleMessage(queue, message));
                    }

                    Thread.Sleep(30 * 1000);
                }
            }
            catch (Exception e)
            {
               _logger.Error(e.ToString());
            }            

            return Task.CompletedTask;
        }

        private void HandleMessage(string queue, string? receivedMessage)
        {
            if (receivedMessage != TEST_MESSAGE)
            {
                _logger.Error("Queue {queue} is NOT working correctly", queue);
                
            }
            else
            {
                SendServerError();
                _logger.Info("Queue {queue} is working correctly", queue);
            }
        }

        private void SendServerError()
        {
            _hubContext.Clients.All.SendAsync("ThrowServerError", "RabbitMQ servers are not working.");
        }
    }
}