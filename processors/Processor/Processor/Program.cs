using System.Text.Json;
using webapp;
using webapp.Json;
using webapp.RabbitMQ;

namespace Processor
{
    internal class Program
    {
        private static readonly string QUEUE_FROM = "queue";
        private static readonly string QUEUE_TO = "post-queue";

        static void Main(string[] args)
        {
            Queue.StartListening(QUEUE_FROM, HandleMessage);

            Console.WriteLine("Processor is listening...");
            Console.ReadLine();
        }

        private static void HandleMessage(string receivedMessage)
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessage>(receivedMessage);

            clientMessage.Message = GetAnswer(clientMessage.Message);

            SendMessageToNextHandler(clientMessage);
        }

        private static string GetAnswer(string message)
        {
            string? answer = JsonHandler.GetAnswer(message);

            answer ??= "Неизвестный вопрос";

            return answer;
        }

        private static void SendMessageToNextHandler(ClientMessage message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            Queue.SendMessage(QUEUE_TO, jsonMessage);
        }
    }
}
