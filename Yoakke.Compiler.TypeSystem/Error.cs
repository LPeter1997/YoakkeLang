using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Compiler.TypeSystem
{
    /// <summary>
    /// The base for every type-error in the library.
    /// </summary>
    public abstract class TypeError : Exception
    {
    }

    /// <summary>
    /// Represents a <see cref="TypeError"/> that occurred because of a recursive type.
    /// </summary>
    public class TypeRecursionError : TypeError
    {
        /// <summary>
        /// The <see cref="Type"/> containing itself.
        /// </summary>
        public readonly Type Container;
        /// <summary>
        /// The <see cref="Type"/> instance referring.
        /// </summary>
        public readonly Type Contained;

        /// <summary>
        /// Initializes a new <see cref="TypeRecursionError"/>.
        /// </summary>
        /// <param name="container">The <see cref="Type"/> containing itself.</param>
        /// <param name="contained">The <see cref="Type"/> instance referring.</param>
        public TypeRecursionError(Type container, Type contained)
        {
            Container = container;
            Contained = contained;
        }
    }

    /// <summary>
    /// Represents a <see cref="TypeError"/> that occurred because of mismatching <see cref="Type"/>s during 
    /// unification.
    /// </summary>
    public class TypeMismatchError : TypeError
    {
        /// <summary>
        /// The first non-matching <see cref="Type"/>.
        /// </summary>
        public readonly Type First;
        /// <summary>
        /// The second non-matching <see cref="Type"/>.
        /// </summary>
        public readonly Type Second;

        /// <summary>
        /// Initializes a new <see cref="TypeMismatchError"/>.
        /// </summary>
        /// <param name="container">The first non-matching <see cref="Type"/>.</param>
        /// <param name="contained">The second non-matching <see cref="Type"/>.</param>
        public TypeMismatchError(Type first, Type second)
        {
            First = first;
            Second = second;
        }
    }
}
