using Cryoptix.Web.API.Constants;

namespace Cryoptix.Web.API.Endpoints
{
    public static class MapCryoptixEndpoints
    {
        public static IEndpointRouteBuilder MapCryoptixApi(this IEndpointRouteBuilder app)
        {
            var apiGroup = app.MapGroup("/api")
                .RequireAuthorization(Claims.CRYOPTIX_CLIENT_ID, Claims.CRYOPTIX_USER_CLAIM);// group-wide policy

            apiGroup.MapAppApi();

            return app;
        }
    }
}
