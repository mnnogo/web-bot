using System.Text.Json;
using webapp.Json;
using webapp.RabbitMQ;

namespace webapp.Models
{
    public class Handler : BackgroundService
    {
        private const string QUEUE_FROM = "queue";
        private const string QUEUE_TO = "post-queue";

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            Queue.StartListening(QUEUE_FROM, HandleMessage);

            return Task.CompletedTask;
        }

        private void HandleMessage(string receivedMessage)
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessage>(receivedMessage);

            clientMessage.Message = GetAnswer(clientMessage.Message);

            SendMessageToNextHandler(clientMessage);
        }

        private string GetAnswer(string message)
        {
            string? answer = JsonHandler.GetAnswer(message);

            answer ??= "Неизвестный вопрос";

            return answer;
        }

        private void SendMessageToNextHandler(ClientMessage message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            Queue.SendMessage(QUEUE_TO, jsonMessage);
        }
    }
}
