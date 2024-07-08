using NLog;
using System.Text.Json;
using webapp;
using webapp.RabbitMQ;

namespace PostProcessor
{
    internal class Program
    {
        private static readonly string QUEUE_FROM = "post-queue";
        private static readonly string QUEUE_TO = "sending-queue";
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Queue.StartListening(QUEUE_FROM, HandleMessage);

            Console.WriteLine("PostProcessor is listening...");
            Console.ReadLine();
        }

        private static void HandleMessage(string receivedMessage)
        {
            var clientMessage = JsonSerializer.Deserialize<ClientMessage>(receivedMessage);

            SendMessageToSendingQueue(clientMessage);
        }

        private static List<string> NormalizeLength(string message, int maxLength)
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

        private static void SendMessageToSendingQueue(ClientMessage receivedMessage)
        {
            var messagesList = NormalizeLength(receivedMessage.Message, 200);

            foreach (var message in messagesList)
            {
                // one message piece + client ID associated with it
                string messagePieceJson = JsonSerializer.Serialize(
                    new ClientMessage(receivedMessage.ClientId, message));

                Queue.SendMessage(QUEUE_TO, messagePieceJson);

                _logger.Info("Message {message} was sent to client {clientId}", // TODO message
                    message, receivedMessage.ClientId);
            }
        }
    }
}
