namespace Cryoptix.Web.API.ExceptionHandling
{
    internal static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Executes the add api exception handling operation.
        /// </summary>
        /// <param name="services">The services value.</param>
        /// <returns>The add api exception handling result.</returns>
        public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
        {
            services.AddProblemDetails(); // enables standard problem details support
            services.AddExceptionHandler<GlobalExceptionHandler>();
            return services;
        }

        /// <summary>
        /// Executes the use api exception handling operation.
        /// </summary>
        /// <param name="app">The app value.</param>
        /// <returns>The use api exception handling result.</returns>
        public static WebApplication UseApiExceptionHandling(this WebApplication app)
        {
            app.UseExceptionHandler(); // must be before endpoints
            return app;
        }
    }
}
