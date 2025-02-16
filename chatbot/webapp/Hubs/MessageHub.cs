﻿using Microsoft.AspNetCore.SignalR;
using webapp.RabbitMQ;
using NLog;
using System.Text.Json;

namespace webapp.Hubs
{
    public class MessageHub : Hub
    {
        private const string QUEUE_TO = "pre-queue";
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

        public Task SendMessage(string message, string connectionId)
        {
            _logger.Info("Message was received from client with connectionId {connectionId}",
                        connectionId);

            string jsonMessage = JsonSerializer.Serialize(
                new ClientMessage(connectionId, message)
                );

            Queue.SendMessage(QUEUE_TO, jsonMessage);

            return Task.CompletedTask;
        }
    }
}
