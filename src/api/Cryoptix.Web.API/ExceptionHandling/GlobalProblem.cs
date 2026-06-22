namespace Cryoptix.Web.API.ExceptionHandling
{
    internal sealed class GlobalProblem
    {
        public int Status { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, object?>? Extensions { get; set; }
    }
}
