namespace Cryoptix.Web.API.ExceptionHandling
{
    public class GlobalProblem
    {
        public int Status { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, object?>? Extensions { get; set; }
    }
}
