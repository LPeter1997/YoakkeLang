using System;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// Pointer type.
        /// </summary>
        public class Ptr : Type
        {
            public readonly Type Subtype;

            public Ptr(Type subtype)
            {
                Subtype = subtype;
            }

            public override string ToTypeString() => $"{Subtype.ToTypeString()}*";
            public override bool Equals(Type? other) => 
                other is Ptr p && Subtype.Equals(p.Subtype);
            public override int GetHashCode() => HashCode.Combine(typeof(Ptr), Subtype);
        }
    }
}
