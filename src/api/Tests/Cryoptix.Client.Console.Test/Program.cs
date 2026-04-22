using Cryoptix.Client.Console.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Console.WriteLine("Cryoptix.Client.Console.Test");
Console.WriteLine();

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddHostedService<NotificationClientWorker>();

var host = builder.Build();

try
{
    Log.Information("Environment: {EnvironmentName}", builder.Environment.EnvironmentName);
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}