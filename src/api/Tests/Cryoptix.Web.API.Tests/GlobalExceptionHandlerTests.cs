using Cryoptix.Web.API.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Cryoptix.Web.API.Tests;

[TestClass]
public sealed class GlobalExceptionHandlerTests
{
    [TestMethod]
    public async Task MapsValidationExceptionToBadRequestProblem()
    {
        // Arrange
        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env.Object);
        DefaultHttpContext http = NewHttpContext();
        var exception = new ValidationException(new ValidationResult("Name is required", ["Name"]), null, null);

        // Act
        bool handled = await handler.TryHandleAsync(http, exception, CancellationToken.None);

        // Assert
        http.Response.Body.Position = 0;
        using JsonDocument json = await JsonDocument.ParseAsync(http.Response.Body, cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(handled);
        Assert.AreEqual(StatusCodes.Status400BadRequest, http.Response.StatusCode);
        Assert.AreEqual(exception.Message, json.RootElement.GetProperty("title").GetString());
        Assert.AreEqual("Name is required", json.RootElement.GetProperty("detail").GetString());
        Assert.AreEqual(http.TraceIdentifier, json.RootElement.GetProperty("TraceId").GetString());
    }

    [TestMethod]
    public async Task HidesServerDetailsOutsideDevelopment()
    {
        // Arrange
        var env = new Mock<IHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns(Environments.Production);
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, env.Object);
        DefaultHttpContext http = NewHttpContext();

        // Act
        bool handled = await handler.TryHandleAsync(http, new InvalidOperationException("secret"), CancellationToken.None);

        // Assert
        http.Response.Body.Position = 0;
        using JsonDocument json = await JsonDocument.ParseAsync(http.Response.Body, cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(handled);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, http.Response.StatusCode);
        Assert.AreEqual("Server error", json.RootElement.GetProperty("title").GetString());
        Assert.AreEqual("An unexpected error occurred.", json.RootElement.GetProperty("detail").GetString());
    }

    [TestMethod]
    public void ExceptionHandlingExtensions_RegisterAndReturnServiceCollection()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        IServiceCollection returned = services.AddApiExceptionHandling();

        // Assert
        Assert.AreSame(services, returned);
        Assert.Contains(d => d.ServiceType.FullName?.Contains("IExceptionHandler") == true, services);
    }

    private static DefaultHttpContext NewHttpContext()
    {
        var services = new ServiceCollection();
        services.AddProblemDetails();
        services.AddLogging();

        DefaultHttpContext context = new()
        {
            RequestServices = services.BuildServiceProvider()
        };

        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "trace-123";
        return context;
    }

    public TestContext TestContext { get; set; }
}
