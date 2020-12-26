using System;

namespace Yoakke.Compiler.Semantic.Types
{
    partial class Type
    {
        /// <summary>
        /// A dependent <see cref="Type"/> read.
        /// </summary>
        public class Dependent : Type
        {
            /// <summary>
            /// The <see cref="Symbol.Var"/> the dependency is caused by.
            /// </summary>
            public readonly Symbol.Var Symbol;

            /// <summary>
            /// Initializes a new <see cref="Dependent"/>.
            /// </summary>
            /// <param name="symbol">The <see cref="Symbol.Var"/> the dependency is caused by.</param>
            public Dependent(Symbol.Var symbol)
                : base(new Scope(ScopeKind.Struct, null))
            {
                Symbol = symbol;
            }

            public override bool Equals(Type? other) => 
                other is Dependent d && ReferenceEquals(Symbol, d.Symbol);
            public override int GetHashCode() => HashCode.Combine(typeof(Dependent), Symbol);
            public override string ToString() => $"val[{Symbol.Name}]";
        }
    }
}
