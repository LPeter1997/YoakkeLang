using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yoakke.Utils;

namespace Yoakke.Semantic
{
    // Constants, constructors

    partial class Type
    {
        // Constants

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

        // Constructors

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

        // Destructuring

        /// <summary>
        /// Destructures this <see cref="Type"/> as a procedure type.
        /// </summary>
        /// <param name="parameters">The parameters of this procedure type.</param>
        /// <param name="returnType">The return <see cref="Type"/> of this procedure.</param>
        public void AsProcedure(out IEnumerable<Type> parameters, out Type returnType)
        {
            var ctor = (Ctor)Substitution;
            Debug.Assert(ctor.Name == "procedure");
            parameters = ctor.Subtypes.Take(ctor.Subtypes.Count - 1);
            returnType = ctor.Subtypes.Last();
        }
    }

    /// <summary>
    /// The base class for types in the compiler.
    /// </summary>
    abstract partial class Type
    {
        /// <summary>
        /// Returns the substitution for this <see cref="Type"/>. Could be itself, if there's no substitution.
        /// </summary>
        public virtual Type Substitution => this;

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains the given <see cref="Var"/>.
        /// </summary>
        /// <param name="typeVariable">The type variable to search for.</param>
        /// <returns>True, if the <see cref="Var"/> is contained.</returns>
        protected abstract bool Contains(Var typeVariable);
    }

    // Operations between types

    partial class Type
    {
        /// <summary>
        /// Tries to unify the given <see cref="Type"/>s.
        /// </summary>
        /// <param name="t1">The first <see cref="Type"/> to unify.</param>
        /// <param name="t2">The second <see cref="Type"/> to unify.</param>
        public static void Unify(Type t1, Type t2)
        {
            void UnifyVarVar(Var v1, Var v2)
            {
                if (ReferenceEquals(v1, v2)) return;
                v2.SubstituteFor(v1);
            }

            void UnifyCtorVar(Ctor c1, Var v2)
            {
                if (c1.Contains(v2))
                {
                    throw new NotImplementedException("Type-recursion!");
                }
                v2.SubstituteFor(c1);
            }

            void UnifyCtorCtor(Ctor c1, Ctor c2)
            {
                if (c1.Name != c2.Name)
                {
                    throw new NotImplementedException("Type mismatch!");
                }
                if (c1.Subtypes.Count != c2.Subtypes.Count)
                {
                    throw new NotImplementedException("Subtype amount mismatch!");
                }
                for (int i = 0; i < c1.Subtypes.Count; ++i)
                {
                    Unify(c1.Subtypes[i], c2.Subtypes[i]);
                }
            }

            t1 = t1.Substitution;
            t2 = t2.Substitution;
            if (t1 is Var v1)
            {
                if (t2 is Var v2)
                {
                    UnifyVarVar(v1, v2);
                    return;
                }
                if (t2 is Ctor c2)
                {
                    UnifyCtorVar(c2, v1);
                    return;
                }
            }
            if (t1 is Ctor c1)
            {
                if (t2 is Var v2)
                {
                    UnifyCtorVar(c1, v2);
                    return;
                }
                if (t2 is Ctor c2)
                {
                    UnifyCtorCtor(c1, c2);
                    return;
                }
            }
            throw new NotImplementedException();
        }

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
            if (t1 is Var || t2 is Var) throw new InvalidOperationException();
            return t1 switch
            {
                Ctor c1 => t2 is Ctor c2
                        && c1.Name == c2.Name
                        && c1.Subtypes.Count == c2.Subtypes.Count
                        && c1.Subtypes.Zip(c2.Subtypes).All(ts => Same(ts.First, ts.Second)),
                _ => throw new NotImplementedException(),
            };
        }
    }

    // Variants

    partial class Type
    {
        /// <summary>
        /// Represents a <see cref="Type"/> that's not inferred yet, and could be substituted for another.
        /// </summary>
        protected class Var : Type
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

            protected override bool Contains(Var typeVariable) =>
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
        private class Ctor : Type
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

            protected override bool Contains(Var typeVariable) =>
                Subtypes.Any(ty => ty.Contains(typeVariable));
        }
    }
}
