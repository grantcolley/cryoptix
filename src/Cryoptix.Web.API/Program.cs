using Cryoptix.Web.API.Authorization;
using Cryoptix.Web.API.Config;
using Cryoptix.Web.API.Constants;
using Cryoptix.Web.API.Endpoints;
using Cryoptix.Web.API.ExceptionHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

string? domain = builder.Configuration[ConfigKeys.AUTH_DOMAIN] ?? throw new NullReferenceException(ConfigKeys.AUTH_DOMAIN);
string? audience = builder.Configuration[ConfigKeys.AUTH_AUDIENCE] ?? throw new NullReferenceException(ConfigKeys.AUTH_AUDIENCE);
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));

builder.Host.UseSerilog((ctx, lc) =>
{
    // %HOME% exists on App Service (Windows + Linux). On Windows it maps to D:\home.
    var home = Environment.GetEnvironmentVariable("HOME") ?? "";
    var logDir = Path.Combine(home, "LogFiles", "Application");
    Directory.CreateDirectory(logDir);

    lc.ReadFrom.Configuration(ctx.Configuration)
      .MinimumLevel.Is(LogEventLevel.Warning)
      .WriteTo.Console()
      .WriteTo.File(
          path: Path.Combine(logDir, "cryoptix-.log"),
          rollingInterval: RollingInterval.Day,
          retainedFileCountLimit: 7,
          shared: true);
});

// Add services to the container.

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = domain;
        options.Audience = audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = domain,
            ValidAudience = audience,
            NameClaimType = "sub",
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Claims.CRYOPTIX_CLIENT_ID, policy =>
    {
        policy.AddRequirements(new AllowedClientRequirement());
    })
    .AddPolicy(Claims.CRYOPTIX_USER_CLAIM, policy =>
    {
        policy.RequireAuthenticatedUser().RequireClaim("permissions", Claims.CRYOPTIX_USER_CLAIM);
    })
    .AddPolicy(Claims.CRYOPTIX_DEVELOPER_CLAIM, policy =>
    {
        policy.RequireAuthenticatedUser().RequireClaim("permissions", Claims.CRYOPTIX_DEVELOPER_CLAIM);
    });

builder.Services.AddApiExceptionHandling();

builder.Services.AddSingleton<IAuthorizationHandler, AllowedClientHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCryoptixApi();

app.Run();