using System.Text.RegularExpressions;
using webapp.RabbitMQ;

namespace webapp.Models
{
    public class PreHandler : BackgroundService
    {
        private const string QUEUE_FROM = "pre-queue";
        private const string QUEUE_TO = "queue";

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            Queue.StartListening(QUEUE_FROM, HandleMessage);           

            return Task.CompletedTask;
        }

        private void HandleMessage(string message)
        {
            message = NormalizeMessage(message);

            SendMessageToNextHandler(message);
        }

        private static string NormalizeMessage(string message)
        {
            string newMessage = message.ToLower().Trim().Replace("  ", " ");

            return Regex.Replace(newMessage, @"[!?.,]", "");
        }
        
        private void SendMessageToNextHandler(string message)
        {
            Queue.SendMessage(QUEUE_TO, message);
        }
    }
}
