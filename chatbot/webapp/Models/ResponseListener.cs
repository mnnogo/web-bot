using Microsoft.AspNetCore.SignalR;
using NLog;
using System.Text.Json;
using webapp.Hubs;
using webapp.RabbitMQ;

namespace webapp.Models
{
    public class ResponseListener : BackgroundService
    {
        private static readonly string QUEUE_FROM = "sending-queue";
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public ResponseListener(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Queue.StartListening(QUEUE_FROM, HandleMessage);

            return Task.CompletedTask;
        }

        private void HandleMessage(string receivedMessage)
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessage>(receivedMessage);

            SendMessageToClient(clientMessage);
        }

        private void SendMessageToClient(ClientMessage message)
        {
            _hubContext.Clients.Client(message.ClientId).SendAsync("ReceiveMessage", message.Message);
            //_logger.Info($"Message '{message}' was sent to client '{receivedMessage.ClientId}'"); // TODO
        }
    }
}
