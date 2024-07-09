using NLog;
using System.Text.Json;

namespace webapp.Json
{
    public class JsonHandler
    {
        private static readonly Logger _logger = LogManager.Setup()
            .LoadConfigurationFromFile("..\\..\\..\\nlog.config").GetCurrentClassLogger();

        // возвращает null если не удалось получить ответ
        public static string? GetAnswer(string message)
        {
            string? answer = null;

            string jsonString = File.ReadAllText("Json/answers.json");

            var items = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (items?.TryGetValue(message, out answer) == false)
            {
                _logger.Debug("No response in JSON. answer = {answer}", answer);
            }
            else
            {
                _logger.Debug("Successfully got response from JSON.");
            }


            return answer;
        }
    }
}
