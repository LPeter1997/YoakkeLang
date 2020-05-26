using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yoakke.Type
{
    abstract class Type
    {
        public static readonly IList<Type> EmptyList = new List<Type>();

        public static readonly Type Unit = Tuple(EmptyList);
        public static readonly Type I32 = Primitive("i32");

        public static Type Variable() =>
            new TypeVariable();

        public static Type Primitive(string name) =>
            new TypeConstructor(name, EmptyList);

        public static Type Tuple(IList<Type> types) =>
            new TypeConstructor("tuple", types);

        public static Type Procedure(IList<Type> parameters, Type ret)
        {
            var types = parameters.ToList();
            types.Add(ret);
            return new TypeConstructor("procedure", types);
        }

        public virtual Type Substitution => this;

        public abstract bool Contains(TypeVariable typeVariable);
    }

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

        public void SubstituteFor(Type type)
        {
            Debug.Assert(substitution == null, "Can only substitute for a type variable once!");
            substitution = type;
        }
    }

    class TypeConstructor : Type
    {
        public string Name { get; }
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
