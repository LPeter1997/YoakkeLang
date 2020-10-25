using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;

namespace Yoakke.Compiler.Semantic.Types
{
    partial class Type
    {
        /// <summary>
        /// A procedure <see cref="Type"/>.
        /// </summary>
        public class Proc : Type
        {
            /// <summary>
            /// The parameter of a <see cref="Proc"/>.
            /// </summary>
            public class Param : IEquatable<Param>
            {
                /// <summary>
                /// Parameter name, if any.
                /// </summary>
                public readonly string? Name;
                /// <summary>
                /// Parameter <see cref="Type"/>.
                /// </summary>
                public readonly Type Type;

                /// <summary>
                /// Initializes a new <see cref="Param"/>.
                /// </summary>
                /// <param name="name">The name of the parameter.</param>
                /// <param name="type">The <see cref="Type"/> of the parameter.</param>
                public Param(string? name, Type type)
                {
                    Name = name;
                    Type = type;
                }

                public override bool Equals(object? obj) => obj is Param p && Equals(p);
                // NOTE: We don't count 'Name' on purpose
                public bool Equals(Param? other) =>  other != null && other.Type.Equals(Type);
                public override int GetHashCode() => HashCode.Combine(Type);
                public override string ToString() => Name == null
                    ? Type.ToString()
                    : $"{Name}: {Type}";
            }

            /// <summary>
            /// The list of parameters.
            /// </summary>
            public readonly IValueList<Param> Parameters;
            /// <summary>
            /// The return <see cref="Type"/>.
            /// </summary>
            public readonly Type Return;

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="parameters">The list of parameters.</param>
            /// <param name="ret">The return <see cref="Type"/>.</param>
            public Proc(IList<Param> parameters, Type ret)
            {
                Parameters = parameters.AsValueList();
                Return = ret;
            }

            public override bool Equals(Type? other) =>
                   other is Proc p
                && Parameters.Equals(p.Parameters)
                && Return.Equals(p.Return);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Proc), Parameters, Return);
            public override string ToString() =>
                $"proc({string.Join(", ", Parameters)}) -> {Return}";
        }
    }
}
