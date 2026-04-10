using Cryoptix.Exchange.Models;

namespace Cryoptix.Strategy.Event
{
    public sealed record MarketEventEnvelope
    {
        public required MarketEventKind Kind { get; init; }
        public required MarketEventSource Source { get; init; }

        public Kline? Kline { get; init; }
        public Trade? Trade { get; init; }
    }
}
