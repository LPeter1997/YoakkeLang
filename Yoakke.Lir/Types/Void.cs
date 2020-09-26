using System;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// Void type.
        /// </summary>
        public class Void : Type
        {
            public override string ToTypeString() => "void";
            public override bool Equals(Type? other) => other is Void;
            public override int GetHashCode() => HashCode.Combine(typeof(Void));
        }
    }
}
