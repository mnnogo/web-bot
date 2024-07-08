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
                    // switch screen to error if file exists
                    ManageErrorsScreens(
                        File.Exists("stop.flag")
                        );

                    Thread.Sleep(30 * 1000);
                }
            });
        }

        private void ManageErrorsScreens(bool switchToError)
        {
            if (switchToError)
            {
                _logger.Error("Stop flag file was detected. Start health monitor or delete the file.");
            }

            _hubContext.Clients.All.SendAsync("SwitchErrorScreens", switchToError);
        }
    }
}
