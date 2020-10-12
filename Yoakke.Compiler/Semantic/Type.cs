using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Syntax;

namespace Yoakke.Compiler.Semantic
{
    /// <summary>
    /// Base class for every type representation.
    /// </summary>
    public abstract partial class Type : IEquatable<Type>
    {
        public static readonly Type I32 = new Prim("i32", Lir.Types.Type.I32);
        public static readonly Type I64 = new Prim("i64", Lir.Types.Type.I64);
        public static readonly Type Bool = new Prim("bool", Lir.Types.Type.I32);
        // NOTE: For now it will do
        public static readonly Type Unit = new Prim("unit", Lir.Types.Type.Void_);
        // Special thing
        public static readonly Type Type_ = new Prim("type", Lir.Types.Type.User_);

        public override bool Equals(object? obj) => obj is Type ty && Equals(ty);

        public abstract bool Equals(Type? other);
        public abstract override int GetHashCode();
    }

    partial class Type
    {
        /// <summary>
        /// A primitive Lir <see cref="Type"/> reference.
        /// </summary>
        public class Prim : Type
        {
            /// <summary>
            /// The name identifier. This is to differentiate different primitives with the same backing Lir type.
            /// </summary>
            public readonly string Name;
            /// <summary>
            /// The Lir <see cref="Type"/>.
            /// </summary>
            public readonly Lir.Types.Type Type;

            /// <summary>
            /// Initializes a new <see cref="Prim"/>.
            /// </summary>
            /// <param name="name">The name identifier.</param>
            /// <param name="type">The Lir <see cref="Type"/> to wrap.</param>
            public Prim(string name, Lir.Types.Type type)
            {
                Name = name;
                Type = type;
            }

            public override bool Equals(Type? other) => 
                other is Prim p && Name == p.Name && Type.Equals(p.Type);
            public override int GetHashCode() => HashCode.Combine(typeof(Prim), Name, Type);
        }

        /// <summary>
        /// A pointer <see cref="Type"/>.
        /// </summary>
        public class Ptr : Type
        {
            /// <summary>
            /// The subtype that the pointer points to.
            /// </summary>
            public readonly Type Subtype;

            /// <summary>
            /// Initializes a new <see cref="Ptr"/>.
            /// </summary>
            /// <param name="subtype">The subtype that the pointer points to.</param>
            public Ptr(Type subtype)
            {
                Subtype = subtype;
            }

            public override bool Equals(Type? other) =>
                   other is Ptr p
                && Subtype.Equals(p.Subtype);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Ptr), Subtype);
        }

        /// <summary>
        /// A procedure <see cref="Type"/>.
        /// </summary>
        public class Proc : Type
        {
            /// <summary>
            /// The list of parameter <see cref="Type"/>s.
            /// </summary>
            public readonly IValueList<Type> Parameters;
            /// <summary>
            /// The return <see cref="Type"/>.
            /// </summary>
            public readonly Type Return;

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="parameters">The list of parameter <see cref="Type"/>s.</param>
            /// <param name="ret">The return <see cref="Type"/>.</param>
            public Proc(IValueList<Type> parameters, Type ret)
            {
                Parameters = parameters;
                Return = ret;
            }

            public override bool Equals(Type? other) =>
                   other is Proc p
                && Parameters.Equals(p.Parameters)
                && Return.Equals(p.Return);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Proc), Parameters, Return);
        }

        /// <summary>
        /// A struct <see cref="Type"/>.
        /// </summary>
        public class Struct : Type
        {
            /// <summary>
            /// The defining 'struct' keyword.
            /// </summary>
            public readonly Token KwStruct;
            /// <summary>
            /// The names to field <see cref="Type"/>s.
            /// </summary>
            public readonly IValueDictionary<string, Type> Fields;

            /// <summary>
            /// Initializes a new <see cref="Struct"/>.
            /// </summary>
            /// <param name="kwStruct">The 'struct' <see cref="Token"/>.</param>
            /// <param name="fields">The names to field <see cref="Type"/>s dictionary.</param>
            public Struct(Token kwStruct, IValueDictionary<string, Type> fields)
            {
                KwStruct = kwStruct;
                Fields = fields;
            }

            public override bool Equals(Type? other) =>
                other is Struct s
                && KwStruct.Equals(s.KwStruct)
                && Fields.Equals(s.Fields);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Struct), KwStruct, Fields);
        }

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
            {
                ElementType = elementType;
                Length = length;
            }

            public override bool Equals(Type? other) =>
                   other is Array a
                && ElementType.Equals(a.ElementType)
                && Length == a.Length;
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Array), ElementType, Length);
        }
    }
}
