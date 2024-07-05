using Microsoft.AspNetCore.SignalR;
using webapp.RabbitMQ;
using NLog;
using System.Text.Json;
using webapp.Hubs;

namespace webapp.Models
{
    public class PostHandler : BackgroundService
    {
        private const string QUEUE_FROM = "post-queue";
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public PostHandler(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            Queue.StartListening(QUEUE_FROM, HandleMessage);

            return Task.CompletedTask;
        }

        private void HandleMessage(string receivedMessage)
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessage>(receivedMessage);

            SendMessageToClient(clientMessage);
        }

        private List<string> NormalizeLength(string message, int maxLength)
        {
            string _message = message;
            List<string> messagesList = [];

            while (_message.Length > 200)
            {
                int separationIndex = _message[..200].LastIndexOf(' ');

                messagesList.Add(_message[..separationIndex]);

                _message = _message[separationIndex..];
            }

            messagesList.Add(_message);

            return messagesList;
        }

        private void SendMessageToClient(ClientMessage receivedMessage)
        {
            var messagesList = NormalizeLength(receivedMessage.Message, 200);

            foreach (var message in messagesList)
            {
                _hubContext.Clients.Client(receivedMessage.ClientId).SendAsync("ReceiveMessage", message);
                _logger.Info($"Message '{message}' was sent to client '{receivedMessage.ClientId}'");
            }
        }
    }
}
