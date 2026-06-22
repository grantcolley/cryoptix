namespace Cryoptix.Web.API.ExceptionHandling
{
    internal sealed class GlobalProblem
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        public Dictionary<string, object?>? Extensions { get; set; }
    }
}
