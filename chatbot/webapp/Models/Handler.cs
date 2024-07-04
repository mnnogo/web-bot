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

        private void HandleMessage(string message)
        {
            string answer = GetAnswer(message);

            SendMessageToNextHandler(answer);
        }

        private string GetAnswer(string message)
        {
            string? answer = JsonHandler.GetAnswer(message);

            answer ??= "неизвестный вопрос";

            return answer;
        }

        private void SendMessageToNextHandler(string message)
        {
            Queue.SendMessage(QUEUE_TO, message);
        }
    }
}
