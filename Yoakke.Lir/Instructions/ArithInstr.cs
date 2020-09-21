using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir.Instructions
{
    /// <summary>
    /// Represents some arithmetic instruction.
    /// </summary>
    public abstract class ArithInstr : ValueInstr
    {
        /// <summary>
        /// The left-hand side of the arithmetic operation.
        /// </summary>
        public Value Left { get; set; }
        /// <summary>
        /// The right-hand side of the arithmetic operation.
        /// </summary>
        public Value Right { get; set; }
        /// <summary>
        /// The keyword of this instruction.
        /// </summary>
        protected abstract string Keyword { get; }

        public override IEnumerable<IInstrArg> InstrArgs
        {
            get
            {
                yield return Result;
                yield return Left;
                yield return Right;
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ArithInstr"/>.
        /// </summary>
        /// <param name="result">The <see cref="Register"/> to store the result in.</param>
        /// <param name="left">The left-hand side operand.</param>
        /// <param name="right">The right-hand side operand.</param>
        public ArithInstr(Register result, Value left, Value right)
            : base(result)
        {
            Left = left;
            Right = right;
        }

        public override string ToString() => $"{Result} = {Keyword} {Left}, {Right}";

        public override void Validate()
        {
            try
            {
                var resultTy = CommonArithmeticType(Left.Type, Right.Type);
                if (!Result.Type.Equals(resultTy))
                {
                    ThrowValidationException("The arithmetic result does not match the result storage type!");
                }
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException(this, "See inner exception.", ex);
            }
        }

        public static Type CommonArithmeticType(Type left, Type right)
        {
            if (left is Type.Int leftInt && right is Type.Int rightInt)
            {
                if (leftInt.Signed != rightInt.Signed)
                {
                    throw new ArgumentException("Integer signedness mismatch!");
                }
                return leftInt.Bits > rightInt.Bits ? leftInt : rightInt;
            }
            else if (left is Type.Int && right is Type.Ptr p)
            {
                return p;
            }
            else if (left is Type.Ptr p2 && right is Type.Int)
            {
                return p2;
            }
            else
            {
                throw new ArgumentException("No common arithmetic type for types!");
            }
        }
    }

    partial class Instr
    {
        // TODO: We could have a way to check or enable wrapping later?

        /// <summary>
        /// Addition instruction.
        /// </summary>
        public class Add : ArithInstr
        {
            public Add(Register result, Value left, Value right) 
                : base(result, left, right)
            {
            }

            protected override string Keyword => "add";
        }

        /// <summary>
        /// Subtraction instruction.
        /// </summary>
        public class Sub : ArithInstr
        {
            public Sub(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "sub";
        }

        /// <summary>
        /// Multiplication instruction.
        /// </summary>
        public class Mul : ArithInstr
        {
            public Mul(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "mul";
        }

        /// <summary>
        /// Division instruction.
        /// </summary>
        public class Div : ArithInstr
        {
            public Div(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "div";
        }

        /// <summary>
        /// Modulo instruction.
        /// </summary>
        public class Mod : ArithInstr
        {
            public Mod(Register result, Value left, Value right)
                : base(result, left, right)
            {
            }

            protected override string Keyword => "mod";
        }
    }
}
