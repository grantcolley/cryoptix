using Cryoptix.Exchange.Api;
using Cryoptix.Strategy.Event;
using System.Threading.Channels;

namespace Cryoptix.Strategy.Seeding
{
    /// <summary>
    /// Defines the strategy market seeder contract.
    /// </summary>
    public interface IStrategyMarketSeeder
    {
        /// <summary>
        /// Executes the seed operation.
        /// </summary>
        /// <param name="strategy">The strategy.</param>
        /// <param name="restApi">The rest API.</param>
        /// <param name="klineWriter">The kline writer.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SeedAsync(
            /// <summary>
            /// Gets the value.
            /// </summary>
            Strategies.Strategy strategy,
            /// <summary>
            /// Gets the value.
            /// </summary>
            IExchangeRestApi restApi,
            /// <summary>
            /// Gets the value.
            /// </summary>
            ChannelWriter<KlineMarketEvent> klineWriter,
            /// <summary>
            /// Gets the value.
            /// </summary>
            Cache.MarketDataCache cache,
            /// <summary>
            /// Gets the value.
            /// </summary>
            CancellationToken cancellationToken);
    }
}
