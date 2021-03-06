﻿using System;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// Array type.
        /// </summary>
        public class Array : Type
        {
            public readonly Type Subtype;
            public readonly int Size;

            public Array(Type subtype, int size)
            {
                Subtype = subtype;
                Size = size;
            }

            public override string ToTypeString() => $"{Subtype.ToTypeString()}[{Size}]";
            public override bool Equals(Type? other) =>
                other is Array a && Subtype.Equals(a.Subtype) && Size == a.Size;
            public override int GetHashCode() => HashCode.Combine(typeof(Array), Subtype, Size);
        }
    }
}
