using System;

namespace Yoakke.Compiler.Semantic.Types
{
    partial class Type
    {
        /// <summary>
        /// An array <see cref="Type"/>.
        /// </summary>
        public class Array : Type
        {
            /// <summary>
            /// The element <see cref="Type"/>.
            /// </summary>
            public readonly Type ElementType;
            /// <summary>
            /// The element count.
            /// </summary>
            public readonly int Length;

            /// <summary>
            /// Initializes a new <see cref="Array"/>.
            /// </summary>
            /// <param name="elementType">The element type.</param>
            /// <param name="length">The length of the array.</param>
            public Array(Type elementType, int length)
                : base(new Scope(ScopeKind.Struct, null))
            {
                ElementType = elementType;
                Length = length;
            }

            protected override bool EqualsExact(Type? other) =>
                   other is Array a
                && ElementType.Equals(a.ElementType)
                && Length == a.Length;
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Array), ElementType, Length);
            public override string ToString() => $"[{Length}]{ElementType}";
        }
    }
}
