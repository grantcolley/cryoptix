using Cryoptix.Web.API.Constants;

namespace Cryoptix.Web.API.Endpoints
{
    internal static class MapCryoptixEndpoints
    {
        public static IEndpointRouteBuilder MapCryoptixApi(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => Results.Ok($"Cryoptix API running {DateTime.Now}"));
            app.MapGet("/health", () => Results.Ok($"Cryoptix is Healthy {DateTime.Now}"));

            var apiGroup = app.MapGroup("/api")
                .RequireAuthorization(Claims.CRYOPTIX_CLIENT_ID, Claims.CRYOPTIX_USER_CLAIM);// group-wide policy

            apiGroup.MapStrategyApi();

            return app;
        }
    }
}
