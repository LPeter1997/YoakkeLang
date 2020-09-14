namespace Yoakke.Lir.Types
{
    partial record Type
    {
        /// <summary>
        /// Void type.
        /// </summary>
        public record Void : Type
        {
            public override string ToString() => "void";
        }
    }
}
