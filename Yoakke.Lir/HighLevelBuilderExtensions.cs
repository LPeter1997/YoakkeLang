using System;
using Yoakke.Lir.Values;

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
    }
}
