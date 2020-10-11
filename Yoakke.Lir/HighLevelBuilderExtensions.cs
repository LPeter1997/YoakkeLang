using System;
using System.Collections.Generic;
using System.Linq;
using Yoakke.Lir.Values;
using Type = Yoakke.Lir.Types.Type;

namespace Yoakke.Lir
{
    /// <summary>
    /// High level extensions for <see cref="Builder"/> to build structural code mode simply.
    /// </summary>
    public static class HighLevelBuilderExtensions
    {
        /// <summary>
        /// Builds an if-then structure inside a <see cref="Builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build inside.</param>
        /// <param name="condition">The <see cref="Func{Builder, Value}"/> that compiles the condition.</param>
        /// <param name="then">The <see cref="Action{Builder}"/> that compiles the 'then' block.</param>
        public static void IfThen(
            this Builder builder, 
            Func<Builder, Value> condition, 
            Action<Builder> then)
        {
            // First we compile the condition
            var conditionValue = condition(builder);
            // We create two new blocks, one for then, one for continuation
            var conditionBB = builder.CurrentBasicBlock;
            var thenBB = builder.DefineBasicBlock("if_then");
            var continueBB = builder.DefineBasicBlock("end_if");
            // We branch to these 2 blocks based on the condition
            builder.CurrentBasicBlock = conditionBB;
            builder.JmpIf(conditionValue, thenBB, continueBB);
            // Populate then
            builder.CurrentBasicBlock = thenBB;
            then(builder);
            // NOTE: continueBB should always exist
            if (!builder.CurrentBasicBlock.EndsInBranch)
            {
                builder.Jmp(continueBB);
            }
            // Continue at the continuation block
            builder.CurrentBasicBlock = continueBB;
        }

        /// <summary>
        /// Builds an if-then-else structure inside a <see cref="Builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build inside.</param>
        /// <param name="condition">The <see cref="Func{Builder, Value}"/> that compiles the condition.</param>
        /// <param name="then">The <see cref="Action{Builder}"/> that compiles the 'then' block.</param>
        /// <param name="else">The <see cref="Action{Builder}"/> that compiles the 'else' block.</param>
        public static void IfThenElse(
            this Builder builder, 
            Func<Builder, Value> condition, 
            Action<Builder> then, 
            Action<Builder> @else)
        {
            // First we compile the condition
            var conditionValue = condition(builder);
            // We create three new blocks, one for then, one for else, one for continuation
            var conditionBB = builder.CurrentBasicBlock;
            var thenBB = builder.DefineBasicBlock("if_then");
            var elseBB = builder.DefineBasicBlock("if_else");
            var continueBB = builder.DefineBasicBlock("end_if");
            // We branch to the if-else blocks based on the condition
            builder.CurrentBasicBlock = conditionBB;
            builder.JmpIf(conditionValue, thenBB, elseBB);
            // Populate then
            builder.CurrentBasicBlock = thenBB;
            then(builder);
            var thenEndsInBranch = builder.CurrentBasicBlock.EndsInBranch;
            if (!thenEndsInBranch)
            {
                builder.Jmp(continueBB);
            }
            // Populate else
            builder.CurrentBasicBlock = elseBB;
            @else(builder);
            var elseEndsInBranch = builder.CurrentBasicBlock.EndsInBranch;
            if (!elseEndsInBranch)
            {
                builder.Jmp(continueBB);
            }
            // Delete continueBB if necessary
            if (thenEndsInBranch && elseEndsInBranch)
            {
                // We don't need continueBB
                builder.RemoveBasicBlock(continueBB);
            }
            else
            {
                // Continue at the continuation block
                builder.CurrentBasicBlock = continueBB;
            }
        }

        /// <summary>
        /// Builds a while structure inside a <see cref="Builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build inside.</param>
        /// <param name="condition">The <see cref="Func{Builder, Value}"/> that compiles the condition.</param>
        /// <param name="body">The <see cref="Action{Builder}"/> that compiles the loop body.</param>
        public static void While(
            this Builder builder,
            Func<Builder, Value> condition,
            Action<Builder> body)
        {
            // First we need a loop condition block, a loop body block and a continuation block
            var lastBB = builder.CurrentBasicBlock;
            var conditionBB = builder.DefineBasicBlock("while_condition");
            var bodyBB = builder.DefineBasicBlock("while_body");
            var continueBB = builder.DefineBasicBlock("end_while");
            // First we need to jump to the loop condition from whenever we are
            builder.CurrentBasicBlock = lastBB;
            builder.Jmp(conditionBB);
            // Compile the condition
            builder.CurrentBasicBlock = conditionBB;
            var conditionValue = condition(builder);
            // Branch based on the condition
            builder.JmpIf(conditionValue, bodyBB, continueBB);
            // Compile the body, jump back to condition checking at the end
            builder.CurrentBasicBlock = bodyBB;
            body(builder);
            // NOTE: We don't remove here otherwise as we need that block, it contains our condition!
            if (!builder.CurrentBasicBlock.EndsInBranch)
            {
                builder.Jmp(conditionBB);
            }
            // Continue at the continuation block
            builder.CurrentBasicBlock = continueBB;
        }

        /// <summary>
        /// Writes out a struct initialization.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build the initialization in.</param>
        /// <param name="structType">The struct <see cref="Type"/> to initialize.</param>
        /// <param name="fieldValues">The pairs of field index and field <see cref="Value"/> to initialize to.</param>
        /// <returns>The pointer <see cref="Value"/> to the allocated and initialized struct.</returns>
        public static Value InitStruct(this Builder builder, Type structType, IEnumerable<KeyValuePair<int, Value>> fieldValues)
        {
            if (!(structType is Struct))
            {
                throw new ArgumentException("The type of a struct initialization must be a struct type!", nameof(structType));
            }
            var structPtr = builder.Alloc(structType);
            foreach (var (idx, value) in fieldValues)
            {
                var fieldPtr = builder.ElementPtr(structPtr, idx);
                builder.Store(fieldPtr, value);
            }
            return structPtr;
        }

        /// <summary>
        /// Writes out an array initialization.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build the initialization in.</param>
        /// <param name="arrayType">The array <see cref="Type"/> to initialize.</param>
        /// <param name="arrayValues">The pairs of field index and field <see cref="Value"/> to initialize to.</param>
        /// <returns>The pointer <see cref="Value"/> to the allocated and initialized array.</returns>
        public static Value InitArray(this Builder builder, Type arrayType, IEnumerable<KeyValuePair<int, Value>> arrayValues)
        {
            if (!(arrayType is Type.Array at))
            {
                throw new ArgumentException("The type of a array initialization must be an array type!", nameof(arrayType));
            }
            var arrayValuePtr = builder.Alloc(arrayType);
            var arrayPtr = builder.Cast(new Type.Ptr(at.Subtype), arrayValuePtr);
            foreach (var (idx, value) in arrayValues)
            {
                var fieldPtr = builder.Add(arrayPtr, Type.I32.NewValue(idx));
                builder.Store(fieldPtr, value);
            }
            return arrayValuePtr;
        }

        /// <summary>
        /// Writes out an array initialization.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build the initialization in.</param>
        /// <param name="elementType">The element <see cref="Type"/> in the array.</param>
        /// <param name="values">The <see cref="Value"/>s to initialize array elements to.</param>
        /// <returns>The pointer <see cref="Value"/> to the allocated and initialized array.</returns>
        public static Value InitArray(this Builder builder, Type elementType, params Value[] values)
        {
            var arrayLen = values.Length;
            var arrayType = new Type.Array(elementType, arrayLen);
            int index = 0;
            return InitArray(
                builder, 
                arrayType, 
                values.Select(v => new KeyValuePair<int, Value>(index++, v)));
        }

        /// <summary>
        /// Builds a lazy 'and' operation that only evaluates the right-hand side if necessary.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build the 'and' in.</param>
        /// <param name="first">The <see cref="Func{Builder, Value}"/> that compiles the first operand to evaluate.</param>
        /// <param name="second">The <see cref="Func{Builder, Value}"/> that compiles the second operand to evaluate.</param>
        /// <returns>The resulting <see cref="Value"/>.</returns>
        public static Value LazyAnd(this Builder builder, Func<Builder, Value> first, Func<Builder, Value> second)
        {
            // NOTE: We assume bool is i32
            var result = builder.Alloc(Type.I32);
            builder.IfThenElse(
                condition: first,
                then: b => b.Store(result, second(b)),
                @else: b => b.Store(result, Type.I32.NewValue(0)));
            return builder.Load(result);
        }

        /// <summary>
        /// Builds a lazy 'or' operation that only evaluates the right-hand side if necessary.
        /// </summary>
        /// <param name="builder">The <see cref="Builder"/> to build the 'or' in.</param>
        /// <param name="first">The <see cref="Func{Builder, Value}"/> that compiles the first operand to evaluate.</param>
        /// <param name="second">The <see cref="Func{Builder, Value}"/> that compiles the second operand to evaluate.</param>
        /// <returns>The resulting <see cref="Value"/>.</returns>
        public static Value LazyOr(this Builder builder, Func<Builder, Value> first, Func<Builder, Value> second)
        {
            // NOTE: We assume bool is i32
            var result = builder.Alloc(Type.I32);
            builder.IfThenElse(
                condition: first,
                then: b => b.Store(result, Type.I32.NewValue(1)),
                @else: b => b.Store(result, second(b)));
            return builder.Load(result);
        }
    }
}
