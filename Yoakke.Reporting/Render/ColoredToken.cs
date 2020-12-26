namespace Yoakke.Reporting.Render
{
    /// <summary>
    /// Represents a single, colored token in the annotated source code.
    /// </summary>
    public struct ColoredToken
    {
        /// <summary>
        /// The start index of the token in the line.
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// The length of the token in characters.
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// The <see cref="TokenKind"/> assigned of the token.
        /// </summary>
        public TokenKind Kind { get; set; }
    }
}
