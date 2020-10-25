using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Semantic.Types
{
    /// <summary>
    /// Base class for every type representation.
    /// </summary>
    public abstract partial class Type : IEquatable<Type>
    {
        public static readonly Type U8 = new Prim("u8", Lir.Types.Type.U8);
        public static readonly Type U16 = new Prim("u16", Lir.Types.Type.U16);
        public static readonly Type U32 = new Prim("u32", Lir.Types.Type.U32);
        public static readonly Type U64 = new Prim("u64", Lir.Types.Type.U64);
        public static readonly Type I8 = new Prim("i8", Lir.Types.Type.I8);
        public static readonly Type I16 = new Prim("i16", Lir.Types.Type.I16);
        public static readonly Type I32 = new Prim("i32", Lir.Types.Type.I32);
        public static readonly Type I64 = new Prim("i64", Lir.Types.Type.I64);

        public static readonly Type Bool = new Prim("bool", Lir.Types.Type.I32);
        // NOTE: For now it will do
        public static readonly Type Unit = new Prim("unit", Lir.Types.Type.Void_);
        // Special thing
        public static readonly Type Type_ = new Prim("type", Lir.Types.Type.User_);

        /// <summary>
        /// The <see cref="Scope"/> this <see cref="Type"/> defines for associated members.
        /// </summary>
        public virtual Scope? DefinedScope => null;

        public override bool Equals(object? obj) => obj is Type t && Equals(t);
        public abstract bool Equals(Type? other);
        public abstract override int GetHashCode();
        public abstract override string ToString();
    }
}
