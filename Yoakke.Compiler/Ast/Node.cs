using System.Collections;
using System.Reflection;
using System.Text;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Syntax;
using Yoakke.Compiler.Utils;

namespace Yoakke.Compiler.Ast
{
    using Type = Yoakke.Compiler.Semantic.Type;

    /// <summary>
    /// Base class for all AST nodes.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The scope this AST node belongs to.
        /// </summary>
        public Scope? Scope { get; set; }

        /// <summary>
        /// Prints the AST into a readable form.
        /// </summary>
        /// <returns>A readable string representation of the AST.</returns>
        public string DumpTree()
        {
            var result = new StringBuilder();
            DumpTree(result, this, 0);
            return result.ToString();
        }

        private static void DumpTree(StringBuilder builder, object value, int indentation)
        {
            // Token
            if (value is Token token)
            {
                builder.Append(token.Value).Append('\n');
                return;
            }
            // List of elements
            if (value is IList list)
            {
                builder.Append('\n');
                foreach (var element in list)
                {
                    if (element == null) continue;

                    Indent(builder, indentation + 1);
                    DumpTree(builder, element, indentation + 2);
                }
                return;
            }
            // Single element
            var type = value.GetType();
            var children = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (children.Length == 1)
            {
                var member = children[0].GetValue(value);
                if (member != null)
                {
                    DumpTree(builder, member, indentation);
                    return;
                }
            }
            // Regular node
            builder.Append($"{type.Name}\n");
            foreach (var memberInfo in children)
            {
                var member = memberInfo.GetValue(value);
                if (member == null) continue;

                Indent(builder, indentation + 1);
                builder.Append($"{memberInfo.Name}: ");
                DumpTree(builder, member, indentation + 2);
            }
        }

        private static void Indent(StringBuilder builder, int amount) =>
            builder.Append(' ', amount * 2);
    }

    /// <summary>
    /// Base class for all statements.
    /// </summary>
    public abstract partial class Statement : Node, ICloneable<Statement>
    {
        /// <summary>
        /// Deep-clones this <see cref="Statement"/>.
        /// </summary>
        /// <returns>The deep-cloned <see cref="Statement"/>.</returns>
        public abstract Statement Clone();
    }

    /// <summary>
    /// Base class for all statements, that can have order-independence.
    /// They are called declarations.
    /// </summary>
    public abstract partial class Declaration : Statement
    {
    }

    /// <summary>
    /// Base class for all expressions, that result in a value and can participate in other expressions.
    /// </summary>
    public abstract partial class Expression : Node, ICloneable<Expression>
    {
        // TODO: The same way we could remove names from IR, we could remove these and other crud from the tree
        // That would make the tree purely syntactic

        /// <summary>
        /// The <see cref="Value"/> this <see cref="Expression"/> evaluates to compile-time, if it's being evaluated
        /// compile-time.
        /// </summary>
        public Value? ConstantValue { get; set; }
        /// <summary>
        /// The <see cref="Type"/> this <see cref="Expression"/> evaluates to.
        /// </summary>
        public Type? EvaluationType { get; set; }

        /// <summary>
        /// Deep-clones this <see cref="Expression"/>.
        /// </summary>
        /// <returns>The deep-cloned <see cref="Expression"/>.</returns>
        public abstract Expression Clone();
    }
}
