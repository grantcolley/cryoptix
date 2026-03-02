namespace Cryoptix.Web.API.Endpoints
{
    public static class MapAppEndpoints
    {
        public static IEndpointRouteBuilder MapAppApi(this IEndpointRouteBuilder app)
        {
            var appGroup = app.MapGroup("/app");

            appGroup.MapGet("/gethealth", GetHealth);

            return app;
        }

        private static async Task<IResult> GetHealth()
        {
            return Results.Ok($"{DateTime.Now} Cryoptix");
        }
    }
}
