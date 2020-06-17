using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Yoakke.IR
{
    /// <summary>
    /// Base for all IR instructions.
    /// </summary>
    abstract partial class Instruction
    {
    }

    /// <summary>
    /// Base for any <see cref="Instruction"/> that stores it's value inside a register.
    /// </summary>
    abstract class ValueInstruction : Instruction
    {
        /// <summary>
        /// The register this <see cref="Instruction"/> returns it's value in.
        /// </summary>
        public Value.Register Value { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ValueInstruction"/>.
        /// </summary>
        /// <param name="value">The register that the <see cref="Instruction"/> returns it's value in.</param>
        public ValueInstruction(Value.Register value)
        {
            Value = value;
        }
    }

    // Instructions

    partial class Instruction
    {
        /// <summary>
        /// An <see cref="Instruction"/> to allocate memory on the stack.
        /// </summary>
        public class Alloc : ValueInstruction
        {
            /// <summary>
            /// The <see cref="Type"/> to allocate space for.
            /// </summary>
            public Type ElementType { get; }

            /// <summary>
            /// Initializes a new <see cref="Alloc"/>.
            /// </summary>
            /// <param name="value">The register that'll contain the pointer to the allocated space.</param>
            public Alloc(Value.Register value)
                : base(value)
            {
                if (value.Type is Type.Ptr ptr)
                {
                    ElementType = ptr.ElementType;
                }
                else
                {
                    throw new ArgumentException("Allocation requires a pointer register type!", nameof(value));
                }
            }
        }

        /// <summary>
        /// Return from the current porcedure.
        /// </summary>
        public class Ret : Instruction
        {
            /// <summary>
            /// The return <see cref="Value"/>, if there's any.
            /// </summary>
            public Value Value { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Ret"/>.
            /// </summary>
            /// <param name="value">The <see cref="Value"/> to return.</param>
            public Ret(Value value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// An <see cref="Instruction"/> to store a given <see cref="Value"/> at a given address.
        /// 
        /// Note, that this is not a <see cref="ValueInstruction"/> on purpose, as the instruction itself does
        /// not produce a value, only uses them.
        /// </summary>
        public class Store : Instruction
        {
            /// <summary>
            /// The target address.
            /// </summary>
            public Value Target { get; set; }
            /// <summary>
            /// The <see cref="Value"/> to store.
            /// </summary>
            public Value Value { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Store"/>.
            /// </summary>
            /// <param name="target">The target address to store the <see cref="Value"/> at.</param>
            /// <param name="value">The <see cref="Value"/> to store.</param>
            public Store(Value target, Value value)
            {
                if (!(target.Type is Type.Ptr))
                {
                    throw new ArgumentException("The target of a load instruction must be a pointer type!", nameof(target));
                }
                // TODO: check if target.ElementType == value.Type
                Target = target;
                Value = value;
            }
        }

        /// <summary>
        /// Loads a <see cref="Value"/> from a given address.
        /// </summary>
        public class Load : ValueInstruction
        {
            /// <summary>
            /// The address to load from.
            /// </summary>
            public Value Source { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Load"/>.
            /// </summary>
            /// <param name="value">The register to store the loaded <see cref="Value"/> in.</param>
            /// <param name="source">The address to load from.</param>
            public Load(Value.Register value, Value source)
                : base(value)
            {
                if (!(source.Type is Type.Ptr))
                {
                    throw new ArgumentException("The source of a load instruction must be a pointer type!", nameof(source));
                }
                // TODO: check if source.ElementType == value.Type
                Source = source;
            }
        }

        /// <summary>
        /// Calls a <see cref="Value"/> with the given argument <see cref="Value"/>s.
        /// </summary>
        public class Call : ValueInstruction
        {
            /// <summary>
            /// The procedure value to call.
            /// </summary>
            public Value Proc { get; set; }
            /// <summary>
            /// The arguments to call the procedure with.
            /// </summary>
            public List<Value> Arguments { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Call"/>.
            /// </summary>
            /// <param name="value">The register to store the result in.</param>
            /// <param name="proc">The procedure to call.</param>
            /// <param name="arguments">The arguments to call the procedure with.</param>
            public Call(Value.Register value, Value proc, List<Value> arguments) 
                : base(value)
            {
                Proc = proc;
                Arguments = arguments;
            }
        }

        /// <summary>
        /// Gets a pointer to an element of another <see cref="Value"/>.
        /// </summary>
        public class ElementPtr : ValueInstruction
        {
            /// <summary>
            /// The source <see cref="Value"/> to load the element pointer of.
            /// </summary>
            public Value Source { get; set; }
            /// <summary>
            /// The index of the element.
            /// </summary>
            public Value Index { get; set; }

            /// <summary>
            /// Initializes a new <see cref="ElementPtr"/>.
            /// </summary>
            /// <param name="value">The register to store the result in.</param>
            /// <param name="source">The source <see cref="Value"/> to load the element pointer of.</param>
            /// <param name="index">The index of the element.</param>
            public ElementPtr(Value.Register value, Value source, Value index) 
                : base(value)
            {
                if (!(source.Type is Type.Ptr))
                {
                    throw new ArgumentException("The source of an element ptr instruction must be a pointer type!", nameof(source));
                }
                if (!(index.Type is Type.Int))
                {
                    throw new ArgumentException("The index of an element ptr instruction must be an integer type!", nameof(index));
                }
                Source = source;
                Index = index;
            }
        }

        /// <summary>
        /// Unconditional jump.
        /// </summary>
        public class Jump : Instruction
        {
            /// <summary>
            /// The <see cref="BasicBlock"/> to jump to.
            /// </summary>
            public BasicBlock Target { get; set; }

            /// <summary>
            /// Initializes a new <see cref="Jump"/>.
            /// </summary>
            /// <param name="target">The target <see cref="BasicBlock"/> to jump to.</param>
            public Jump(BasicBlock target)
            {
                Target = target;
            }
        }

        public class JumpIf : Instruction
        {
            /// <summary>
            /// The condition that decides the jump target.
            /// </summary>
            public Value Condition { get; set; }
            /// <summary>
            /// The <see cref="BasicBlock"/> to jump to when the <see cref="Condition"/> is true.
            /// </summary>
            public BasicBlock Then { get; set; }
            /// <summary>
            /// The <see cref="BasicBlock"/> to jump to when the <see cref="Condition"/> is false.
            /// </summary>
            public BasicBlock Else { get; set; }

            /// <summary>
            /// Initializes a new <see cref="JumpIf"/>.
            /// </summary>
            /// <param name="condition">The condition that decides the jump target.</param>
            /// <param name="then">The <see cref="BasicBlock"/> to jump to when the condition is true.</param>
            /// <param name="els">The <see cref="BasicBlock"/> to jump to when the condition is false.</param>
            public JumpIf(Value condition, BasicBlock then, BasicBlock els)
            {
                Condition = condition;
                Then = then;
                Else = els;
            }
        }
    }
}
