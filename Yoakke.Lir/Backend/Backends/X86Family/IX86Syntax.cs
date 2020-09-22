namespace Yoakke.Lir.Backend.Backends.X86Family
{
    /// <summary>
    /// An interface for abstracting away intel and AT&T syntax.
    /// </summary>
    public interface IX86Syntax
    {
        /// <summary>
        /// Returns the intel representation.
        /// </summary>
        /// <param name="formatOptions">The <see cref="X86FormatOptions"/> to use.</param>
        /// <returns>The string of the intel representation.</returns>
        public string ToIntelSyntax(X86FormatOptions formatOptions);
    }

    /// <summary>
    /// Format options for x86 syntax.
    /// </summary>
    public class X86FormatOptions
    {
        /// <summary>
        /// True, if the instruction names and registers thould be all upper case.
        /// </summary>
        public bool AllUpperCase { get; set; }
        /// <summary>
        /// The special separator character.
        /// </summary>
        public char SpecialSeparator { get; set; }
        /// <summary>
        /// The comment character.
        /// </summary>
        public char Comment { get; set; }
        /// <summary>
        /// True, if the comment should go above the instruction, false of after the instruction.
        /// </summary>
        public bool CommentAbove { get; set; }
    }
}
