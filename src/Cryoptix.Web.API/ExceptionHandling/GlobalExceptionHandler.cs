using Microsoft.AspNetCore.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace Cryoptix.Web.API.ExceptionHandling
{
    public sealed class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;
        private readonly IHostEnvironment _env = env;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception");

            GlobalProblem globalProblem = MapException(httpContext, exception);

            // Avoid leaking internal detail in production for 5xx
            var detail =
                globalProblem.Status >= 500 && !_env.IsDevelopment()
                    ? "An unexpected error occurred."
                    : exception.Message;

            var problem = Results.Problem(
                title: globalProblem.Title,
                detail: detail,
                statusCode: globalProblem.Status,
                extensions: globalProblem.Extensions);

            await problem.ExecuteAsync(httpContext);

            return true; // if returns false exceptions continue bubbling up
        }

        private static GlobalProblem MapException(HttpContext ctx, Exception ex)
        {
            var traceId = ctx.TraceIdentifier;

            var extensions = new Dictionary<string, object?>
            {
                ["TraceId"] = traceId
            };

            // Handle specific exceptions here e.g. ....
            if (ex is ValidationException ve)
            {
                extensions["ErrorMessage}"] = ve.ValidationResult.ErrorMessage;
                extensions["MemberNames"] = string.Join(",", ve.ValidationResult.MemberNames);

                return new GlobalProblem
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = ve.Message,
                    Extensions = extensions
                };
            }

            // Everything else is unexpected
            return new GlobalProblem
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server error",
                Extensions = extensions
            };
        }
    }
}
