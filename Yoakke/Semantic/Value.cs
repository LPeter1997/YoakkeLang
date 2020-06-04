using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
{
    /// <summary>
    /// Represents a compile-time constant.
    /// </summary>
    abstract class Value
    {
        /// <summary>
        /// The <see cref="Type"/> of this constant value.
        /// </summary>
        public abstract Type Type { get; }
    }

    /// <summary>
    /// A compile-time <see cref="Value"/> that stores a <see cref="Type"/>.
    /// </summary>
    class TypeValue : Value
    {
        public override Type Type => Type.Type_;
        /// <summary>
        /// The <see cref="Type"/> this <see cref="TypeValue"/> stores.
        /// </summary>
        public Type Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="TypeValue"/>.
        /// </summary>
        /// <param name="value">The <see cref="Type"/> this value stores.</param>
        public TypeValue(Type value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// A procedure as a compile-time <see cref="Value"/>.
    /// </summary>
    class ProcValue : Value
    {
        private Type type;
        public override Type Type => type;

        /// <summary>
        /// The AST node of the procedure.
        /// </summary>
        public readonly ProcExpression Node;

        /// <summary>
        /// Initializes a new <see cref="ProcValue"/>.
        /// </summary>
        /// <param name="node">The AST node this procedure originates from.</param>
        /// <param name="type">The <see cref="Type"/> of this procedure.</param>
        public ProcValue(ProcExpression node, Type type)
        {
            Node = node;
            this.type = type;
        }
    }

    /// <summary>
    /// A compile-time integral <see cref="Value"/>.
    /// </summary>
    class IntValue : Value
    {
        private readonly Type type;
        public override Type Type => type;

        /// <summary>
        /// The integer value.
        /// </summary>
        public readonly BigInteger Value;

        /// <summary>
        /// Initializes a new <see cref="IntValue"/>.
        /// </summary>
        /// <param name="type">The type of the integer.</param>
        /// <param name="value">The value of the integer.</param>
        public IntValue(Type type, BigInteger value)
        {
            this.type = type;
            Value = value;
        }
    }
}
