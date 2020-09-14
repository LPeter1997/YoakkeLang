namespace Yoakke.Text
{
    /// <summary>
    /// A pair of <see cref="Span"/> and an associated value.
    /// </summary>
    /// <typeparam name="TValue">The associated balue type.</typeparam>
    public readonly struct SpanValuePair<TValue>
    {
        /// <summary>
        /// The <see cref="Span"/>.
        /// </summary>
        public readonly Span Span;
        /// <summary>
        /// The value associated to the <see cref="Span"/>.
        /// </summary>
        public readonly TValue Value;

        /// <summary>
        /// Initializes a new <see cref="Span"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="value">The value associated to the <see cref="Span"/></param>
        public SpanValuePair(Span span, TValue value)
        {
            Span = span;
            Value = value;
        }

        public void Deconstruct(out Span span, out TValue value)
        {
            span = Span;
            value = Value;
        }
    }
}
