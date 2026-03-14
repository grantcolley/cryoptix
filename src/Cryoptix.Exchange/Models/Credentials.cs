namespace Cryoptix.Exchange.Models
{
    public class Credentials
    {
        public Exchanges.Exchange Exchange { get; set; }
        public string? AccountName { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? ApiPassPhrase { get; set; }
    }
}