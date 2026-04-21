using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog;

Console.WriteLine("Cryoptix.Client.Console.Test");
Console.WriteLine();

var hubUrl = "https://localhost:7040/api/strategy/subscribe";

// Paste your Auth0 JWT here
var bearerToken = "";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var connection = new HubConnectionBuilder()
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

// Strongly typed handler (recommended)
connection.On<NotificationEnvelope>("ReceiveNotification", message =>
{
    Log.Information("Received notification {MessageType} at {Timestamp}",
        message.MessageType,
        message.TimestampUtc);

    Log.Debug("Payload: {Payload}", message.Payload);
});

// Connection lifecycle logging
connection.Reconnecting += error =>
{
    Log.Warning(error, "Reconnecting...");
    return Task.CompletedTask;
};

connection.Reconnected += connectionId =>
{
    Log.Information("Reconnected. ConnectionId: {ConnectionId}", connectionId);
    return Task.CompletedTask;
};

connection.Closed += async error =>
{
    Log.Error(error, "Connection closed");

    await Task.Delay(2000);
    await connection.StartAsync();
};

try
{
    Log.Information("Connecting to {HubUrl}", hubUrl);

    await connection.StartAsync();

    Log.Information("Connected. ConnectionId: {ConnectionId}", connection.ConnectionId);

    await Task.Delay(Timeout.Infinite);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Connection failed");
}
finally
{
    Log.CloseAndFlush();
}

// Model
public sealed class NotificationEnvelope
{
    public string MessageType { get; set; } = default!;
    public DateTime TimestampUtc { get; set; }
    public string Payload { get; set; } = default!;
}

