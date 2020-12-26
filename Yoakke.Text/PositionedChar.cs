namespace Yoakke.Text
{
    /// <summary>
    /// Represents a character that has a <see cref="Position"/>.
    /// </summary>
    public readonly struct PositionedChar
    {
        /// <summary>
        /// The <see cref="Position"/> of the character.
        /// </summary>
        public readonly Position Position;
        /// <summary>
        /// The character.
        /// </summary>
        public readonly char Char;

        public PositionedChar(Position position, char ch)
        {
            Position = position;
            Char = ch;
        }
    }
}
