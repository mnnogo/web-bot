using NLog;
using System.Text.Json;

namespace webapp.Json
{
    public class JsonHandler
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
               
        // возвращает null если не удалось получить ответ
        public static string? GetAnswer(string message)
        {
            string? answer = null;

            string jsonString = File.ReadAllText("Json/answers.json");

            var items = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (items == null || !items.TryGetValue(message, out answer))
            {
                _logger.Warn("Trying to get value from JSON. items = {items}, answer = {answer}",
                              items, answer);
            }
             
            return answer;
        }
    }
}
