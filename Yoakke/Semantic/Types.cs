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
    abstract class Type
    {
        /// <summary>
        /// Constant for an empty <see cref="Type"/> list.
        /// </summary>
        public static readonly IList<Type> EmptyList = new List<Type>();

        /// <summary>
        /// Unit type, used as no return type.
        /// </summary>
        public static readonly Type Unit = Tuple(EmptyList);
        /// <summary>
        /// 32-bit signed integer type.
        /// </summary>
        public static readonly Type I32 = Primitive("i32");

        /// <summary>
        /// Creates a <see cref="TypeVariable"/>.
        /// </summary>
        /// <returns>A new, unique <see cref="TypeVariable"/>.</returns>
        public static Type Variable() =>
            new TypeVariable();

        /// <summary>
        /// Creates a primitive type.
        /// </summary>
        /// <param name="name">The name of the primitive type.</param>
        /// <returns>The <see cref="TypeConstructor"/> representing a primitive type.</returns>
        public static Type Primitive(string name) =>
            new TypeConstructor(name, EmptyList);

        /// <summary>
        /// Creates a tuple type.
        /// </summary>
        /// <param name="types">The list of <see cref="Type"/>s the tuple consists of.</param>
        /// <returns>The <see cref="TypeConstructor"/> representing a tuple type.</returns>
        public static Type Tuple(IList<Type> types) =>
            new TypeConstructor("tuple", types);

        /// <summary>
        /// Creates a procedure type.
        /// </summary>
        /// <param name="parameters">The list of parameter <see cref="Type"/>s.</param>
        /// <param name="ret">The return <see cref="Type"/>.</param>
        /// <returns>The <see cref="TypeConstructor"/> representing a procedure type.</returns>
        public static Type Procedure(IList<Type> parameters, Type ret)
        {
            var types = parameters.ToList();
            types.Add(ret);
            return new TypeConstructor("procedure", types);
        }

        /// <summary>
        /// Returns the substitution for this <see cref="Type"/>. Could be itself, if there's no substitution.
        /// </summary>
        public virtual Type Substitution => this;

        /// <summary>
        /// Checks, if this <see cref="Type"/> contains the given <see cref="TypeVariable"/>.
        /// </summary>
        /// <param name="typeVariable">The type variable to search for.</param>
        /// <returns>True, if the <see cref="TypeVariable"/> is contained.</returns>
        public abstract bool Contains(TypeVariable typeVariable);
    }

    /// <summary>
    /// Represents a <see cref="Type"/> that's not inferred yet, and could be substituted for another.
    /// </summary>
    class TypeVariable : Type
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

        public override bool Contains(TypeVariable typeVariable) =>
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
    class TypeConstructor : Type
    {
        /// <summary>
        /// The name of this type.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The subtypes of this type.
        /// </summary>
        public IList<Type> Subtypes { get; }

        internal TypeConstructor(string name, IList<Type> subtypes)
        {
            Name = name;
            Subtypes = subtypes;
        }

        public override bool Contains(TypeVariable typeVariable) =>
            Subtypes.Any(ty => ty.Contains(typeVariable));
    }
}
