using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Cryoptix.Client.Console.Test
{
    public sealed class NotificationClientWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationClientWorker> _logger;
        private HubConnection? _connection;

        public NotificationClientWorker(
            IConfiguration configuration,
            ILogger<NotificationClientWorker> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var hubUrl = _configuration["SignalR:HubUrl"]
                ?? throw new InvalidOperationException("SignalR:HubUrl is not configured.");

            var bearerToken = _configuration["Auth:BearerToken"]
                ?? throw new InvalidOperationException("Auth:BearerToken is not configured.");

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(bearerToken)!;
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<NotificationEnvelope>("ReceiveNotification", message =>
            {
                _logger.LogInformation(
                    "Received notification {MessageType} at {TimestampUtc}",
                    message.MessageType,
                    message.TimestampUtc);

                string? payload = message.Payload != null
                    ? System.Text.Json.JsonSerializer.Serialize(message.Payload, new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    })
                    : null;

                _logger.LogInformation("Payload: {Payload}", payload);
            });

            _connection.Reconnecting += error =>
            {
                _logger.LogWarning(error, "Reconnecting to SignalR hub");
                return Task.CompletedTask;
            };

            _connection.Reconnected += connectionId =>
            {
                _logger.LogInformation("Reconnected to SignalR hub. ConnectionId: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                _logger.LogWarning(error, "SignalR connection closed");
                return Task.CompletedTask;
            };

            _logger.LogInformation("Connecting to {HubUrl}", hubUrl);
            await _connection.StartAsync(cancellationToken);
            _logger.LogInformation("Connected. ConnectionId: {ConnectionId}", _connection.ConnectionId);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Notification client worker is stopping");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_connection is not null)
            {
                _logger.LogInformation("Stopping SignalR connection");

                await _connection.DisposeAsync();
                _connection = null;
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
