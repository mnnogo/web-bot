using Microsoft.AspNetCore.SignalR;
using webapp.RabbitMQ;
using NLog;

namespace webapp.Models
{
    public class MessageHub : Hub
    {
        private const string QUEUE_TO = "pre-queue";
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public Task SendMessage(string message)
        {
            _logger.Info($"Message '{message}' was received from client");
            Queue.SendMessage(QUEUE_TO, message);

            return Task.CompletedTask;
        }
    }
}
