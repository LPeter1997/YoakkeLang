namespace Yoakke.Text
{
    /// <summary>
    /// Represents a character that has a <see cref="Position"/> inside some <see cref="SourceFile"/>.
    /// </summary>
    public readonly struct PositionedChar
    {
        /// <summary>
        /// The <see cref="SourceFile"/> the character is in.
        /// </summary>
        public readonly SourceFile? SourceFile;
        /// <summary>
        /// The <see cref="Position"/> of the character.
        /// </summary>
        public readonly Position Position;
        /// <summary>
        /// The character.
        /// </summary>
        public readonly char Char;

        public PositionedChar(SourceFile? sourceFile, Position position, char ch)
        {
            SourceFile = sourceFile;
            Position = position;
            Char = ch;
        }
    }
}
