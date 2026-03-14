namespace Cryoptix.Web.API.ExceptionHandling
{
    public static class ExceptionHandlingExtensions
    {
        public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
        {
            services.AddProblemDetails(); // enables standard problem details support
            services.AddExceptionHandler<GlobalExceptionHandler>();
            return services;
        }

        public static WebApplication UseApiExceptionHandling(this WebApplication app)
        {
            app.UseExceptionHandler(); // must be before endpoints
            return app;
        }
    }
}
