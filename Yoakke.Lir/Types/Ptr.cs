namespace Yoakke.Lir.Types
{
    partial record Type
    {
        /// <summary>
        /// Pointer type.
        /// </summary>
        public record Ptr(Type Subtype) : Type
        {
            public override string ToString() => $"{Subtype}*";
        }
    }
}
