
using Microsoft.AspNetCore.SignalR;
using NLog;
using webapp.Hubs;

namespace webapp.Models
{
    public class StopFlagChecker : BackgroundService
    {
        private IHubContext<MessageHub> _hubContext;
        private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public StopFlagChecker(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (File.Exists("stop.flag"))
                    {
                        SendServerError();
                    }

                    Thread.Sleep(30 * 1000);
                }
            });
        }

        private void SendServerError()
        {
            _logger.Error("Stop flag file was detected. Start health monitor or delete the file.");
            _hubContext.Clients.All.SendAsync("ThrowServerError", "RabbitMQ servers are not working.");
        }
    }
}
