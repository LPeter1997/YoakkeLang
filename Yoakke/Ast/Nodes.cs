using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Yoakke.Syntax;
using Yoakke.Type;

namespace Yoakke.Ast
{
    using Type = Yoakke.Type.Type;

    /// <summary>
    /// Base class for all AST nodes.
    /// </summary>
    abstract class Node
    {
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
    abstract class Statement : Node
    {
    }

    /// <summary>
    /// Base class for all statements, that can have order-independence.
    /// They are called declarations.
    /// </summary>
    abstract class Declaration : Statement
    {
    }

    /// <summary>
    /// Base class for all expressions, that result in a value and can participate in other expressions.
    /// </summary>
    abstract class Expression : Node
    {
        /// <summary>
        /// The <see cref="Type"/> this <see cref="Expression"/> evaluates to.
        /// </summary>
        public Type? Type { get; set; }
    }
}
