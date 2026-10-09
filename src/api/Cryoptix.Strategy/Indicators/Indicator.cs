namespace Cryoptix.Strategy.Indicators
{
    /// <summary>
    /// Represents a configured strategy indicator.
    /// </summary>
    public class Indicator
    {
        /// <summary>
        /// Gets or sets the persistent identifier, independent of the display name.
        /// </summary>
        /// <remarks>
        /// Preserve this identifier when renaming the indicator or changing its type.
        /// Assign a new identifier when creating a new indicator.
        /// </remarks>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the optional display name. Names do not need to be unique.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the indicator type that determines its required parameters.
        /// </summary>
        public IndicatorType IndicatorType { get; set; }

        /// <summary>
        /// Gets or sets the integer parameters in indicator definition order.
        /// </summary>
        /// <remarks>
        /// The parameter types must exactly match the selected indicator type.
        /// An indicator of type None has no parameters.
        /// </remarks>
        public List<IndicatorValue> Values { get; set; } = new();
    }
}
