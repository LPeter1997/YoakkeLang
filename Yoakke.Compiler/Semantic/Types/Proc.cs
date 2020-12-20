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
            // TODO: Doc
            public class Dependency
            {
                public readonly IReadOnlyList<int> DependeeIndices;
                public readonly IReadOnlyList<int> IndependentIndices;
                public readonly IReadOnlyList<int> DependentIndices;

                public Dependency(
                    IReadOnlyList<int> dependee, 
                    IReadOnlyList<int> independent, 
                    IReadOnlyList<int> dependent)
                {
                    DependeeIndices = dependee;
                    IndependentIndices = independent;
                    DependentIndices = dependent;
                }
            }

            /// <summary>
            /// The parameter of a <see cref="Proc"/>.
            /// </summary>
            public class Param : IEquatable<Param>
            {
                /// <summary>
                /// Parameter <see cref="Symbol"/>.
                /// </summary>
                public readonly Symbol Symbol;
                /// <summary>
                /// Parameter <see cref="Type"/>.
                /// </summary>
                public readonly Type Type;

                /// <summary>
                /// Initializes a new <see cref="Param"/>.
                /// </summary>
                /// <param name="symbol">The <see cref="Symbol"/> of the parameter.</param>
                /// <param name="type">The <see cref="Type"/> of the parameter.</param>
                public Param(Symbol symbol, Type type)
                {
                    Symbol = symbol;
                    Type = type;
                }

                public override bool Equals(object? obj) => obj is Param p && Equals(p);
                // NOTE: We don't count 'Symbol' on purpose
                public bool Equals(Param? other) =>  other != null && other.Type.Equals(Type);
                public override int GetHashCode() => HashCode.Combine(Type);
                public override string ToString() => $"{Symbol.Name}: {Type}";
            }

            /// <summary>
            /// True, if this is an intrinsic procedure type and should be handled specially.
            /// </summary>
            public readonly bool IsIntrinsic;
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
            /// <param name="isIntrinsic">True, if this is an instrinsic procedure type.</param>
            /// <param name="parameters">The list of parameters.</param>
            /// <param name="ret">The return <see cref="Type"/>.</param>
            public Proc(bool isIntrinsic, IList<Param> parameters, Type ret)
                : base(new Scope(ScopeKind.Struct, null))
            {
                IsIntrinsic = isIntrinsic;
                Parameters = parameters.AsValueList();
                Return = ret;
            }

            /// <summary>
            /// Initializes a new <see cref="Proc"/>.
            /// </summary>
            /// <param name="parameters">The list of parameters.</param>
            /// <param name="ret">The return <see cref="Type"/>.</param>
            public Proc(IList<Param> parameters, Type ret)
                : this(false, parameters, ret)
            {
            }

            public override bool Equals(Type? other) =>
                   other is Proc p
                && Parameters.Equals(p.Parameters)
                && Return.Equals(p.Return);
            public override int GetHashCode() =>
                HashCode.Combine(typeof(Proc), Parameters, Return);
            public override string ToString() =>
                $"proc({string.Join(", ", Parameters)}) -> {Return}";

            // TODO: Doc
            public Dependency? GetDependency()
            {
                var dependentSymbols = Parameters
                    .Select(p => p.Type)
                    .Append(Return)
                    .OfType<Dependent>()
                    .Select(t => (Symbol)t.Symbol)
                    .Where(sym => Parameters.Any(p => p.Symbol == sym))
                    .ToHashSet();
                if (dependentSymbols.Count == 0) return null;

                var paramsWithIndices = Parameters.Select((p, i) => (Param: p, Index: i));
                var dependeeParams = paramsWithIndices
                    .Where(pi => dependentSymbols.Contains(pi.Param.Symbol))
                    .Select(pi => pi.Index)
                    .ToArray();
                var dependentParams = paramsWithIndices
                    .Where(pi => pi.Param.Type is Dependent)
                    .Select(pi => pi.Index)
                    .ToArray();
                var independentParams = paramsWithIndices
                    .Select(pi => pi.Index)
                    .Where(i => !dependeeParams.Contains(i) && !dependentParams.Contains(i))
                    .ToArray();

                return new Dependency(
                    dependeeParams,
                    independentParams,
                    dependentParams);
            }
        }
    }
}
