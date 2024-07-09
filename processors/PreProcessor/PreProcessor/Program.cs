using NLog;
using System.Text.Json;
using System.Text.RegularExpressions;
using webapp;
using webapp.RabbitMQ;

internal class Program
{
    private static readonly string QUEUE_FROM = "pre-queue";
    private static readonly string QUEUE_TO = "queue";

    private static readonly ILogger _logger = LogManager.Setup()
        .LoadConfigurationFromFile("..\\..\\..\\nlog.config").GetCurrentClassLogger();

    private static void Main(string[] args)
    {
        Queue.StartListening(QUEUE_FROM, HandleMessage);

        Console.WriteLine("PreProcessor is listening...");
        Console.ReadLine();
    }

    private static void HandleMessage(string receivedMessage)
    {
        var clientMessage = JsonSerializer.Deserialize<ClientMessage>(receivedMessage);

        clientMessage.Message = NormalizeMessage(clientMessage.Message);

        SendMessageToNextHandler(clientMessage);
    }

    private static string NormalizeMessage(string message)
    {
        string newMessage = message.ToLower().Trim().Replace("  ", " ");

        return Regex.Replace(newMessage, @"[!?.,]", "");
    }

    private static void SendMessageToNextHandler(ClientMessage message)
    {
        string jsonMessage = JsonSerializer.Serialize(message);
        Queue.SendMessage(QUEUE_TO, jsonMessage);
    }
}