using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoakke.Semantic
{
    /// <summary>
    /// The base class for types in the compiler.
    /// </summary>
    abstract partial class Type
    {
        /// <summary>
        /// Constant for an empty <see cref="Type"/> list.
        /// </summary>
        public static readonly IList<Type> EmptyList = new List<Type>();

        /// <summary>
        /// The type of a type
        /// </summary>
        public static readonly Type Type_ = Primitive("type");
        /// <summary>
        /// Unit type, used as no return type.
        /// </summary>
        public static readonly Type Unit = Tuple(EmptyList);
        // TODO: We could make integer types dependent types too, where the dependant int would describe bit-width
        /// <summary>
        /// 32-bit signed integer type.
        /// </summary>
        public static readonly Type I32 = Primitive("i32");

        /// <summary>
        /// Creates a <see cref="Var"/>.
        /// </summary>
        /// <returns>A new, unique <see cref="Var"/>.</returns>
        public static Type Variable() =>
            new Var();

        /// <summary>
        /// Creates a primitive type.
        /// </summary>
        /// <param name="name">The name of the primitive type.</param>
        /// <returns>The <see cref="Ctor"/> representing a primitive type.</returns>
        public static Type Primitive(string name) =>
            new Ctor(name, EmptyList);

        /// <summary>
        /// Creates a tuple type.
        /// </summary>
        /// <param name="types">The list of <see cref="Type"/>s the tuple consists of.</param>
        /// <returns>The <see cref="Ctor"/> representing a tuple type.</returns>
        public static Type Tuple(IList<Type> types) =>
            new Ctor("tuple", types);

        /// <summary>
        /// Creates a procedure type.
        /// </summary>
        /// <param name="parameters">The list of parameter <see cref="Type"/>s.</param>
        /// <param name="ret">The return <see cref="Type"/>.</param>
        /// <returns>The <see cref="Ctor"/> representing a procedure type.</returns>
        public static Type Procedure(IList<Type> parameters, Type ret)
        {
            var types = parameters.ToList();
            types.Add(ret);
            return new Ctor("procedure", types);
        }

        /// <summary>
        /// Returns the substitution for this <see cref="Type"/>. Could be itself, if there's no substitution.
        /// </summary>
        public virtual Type Substitution => this;

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains the given <see cref="Var"/>.
        /// </summary>
        /// <param name="typeVariable">The type variable to search for.</param>
        /// <returns>True, if the <see cref="Var"/> is contained.</returns>
        public abstract bool Contains(Var typeVariable);

        /// <summary>
        /// Checks wether two <see cref="Type"/>s are exactly the same.
        /// This should be used after type-inference is done.
        /// </summary>
        /// <param name="t1">The first <see cref="Type"/> to compare.</param>
        /// <param name="t2">The second <see cref="Type"/> to compare.</param>
        /// <returns>True, if the two <see cref="Type"/>s are equal.</returns>
        public static bool Same(Type t1, Type t2)
        {
            t1 = t1.Substitution;
            t2 = t2.Substitution;
            if (t1 is Var) throw new InvalidOperationException();
            return t1 switch
            {
                Var _ => throw new InvalidOperationException(),
                Ctor c1 => t2 is Ctor c2 
                                   && c1.Name == c2.Name 
                                   && c1.Subtypes.Count == c2.Subtypes.Count
                                   && c1.Subtypes.Zip(c2.Subtypes).All((ts) => Same(ts.First, ts.Second)),
                _ => throw new NotImplementedException(),
            };
        }
    }

    partial class Type
    {
        /// <summary>
        /// Represents a <see cref="Type"/> that's not inferred yet, and could be substituted for another.
        /// </summary>
        public class Var : Type
        {
            private Type? substitution;
            public override Type Substitution
            {
                get
                {
                    if (substitution == null) return this;
                    // Pruning
                    substitution = substitution.Substitution;
                    return substitution;
                }
            }

            public override bool Contains(Var typeVariable) =>
                ReferenceEquals(this, typeVariable);

            /// <summary>
            /// Substitutes this type variable for another <see cref="Type"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to substitute for.</param>
            public void SubstituteFor(Type type)
            {
                Debug.Assert(substitution == null, "Can only substitute for a type variable once!");
                substitution = type;
            }
        }

        /// <summary>
        /// Represents a concrete <see cref="Type"/> with a name and possible subtypes.
        /// </summary>
        public class Ctor : Type
        {
            /// <summary>
            /// The name of this type.
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// The subtypes of this type.
            /// </summary>
            public IList<Type> Subtypes { get; }

            internal Ctor(string name, IList<Type> subtypes)
            {
                Name = name;
                Subtypes = subtypes;
            }

            public override bool Contains(Var typeVariable) =>
                Subtypes.Any(ty => ty.Contains(typeVariable));
        }
    }
}
