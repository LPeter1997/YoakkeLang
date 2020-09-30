using System.Collections;
using System.Reflection;
using System.Text;

namespace Yoakke.Syntax.Ast
{
    /// <summary>
    /// The base class for every AST node.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The parse tree's node this one originates from.
        /// </summary>
        public readonly ParseTree.Node? ParseTreeNode;

        public Node(ParseTree.Node? parseTreeNode)
        {
            ParseTreeNode = parseTreeNode;
        }

        /// <summary>
        /// Debug dump.
        /// </summary>
        public string Dump()
        {
            var sb = new StringBuilder();
            Dump(sb, 0);
            return sb.ToString();
        }

        private static void Indent(StringBuilder builder, int amount) => 
            builder.Append(new string(' ', amount * 2));

        private void Dump(StringBuilder builder, int indent)
        {
            var type = GetType();

            // First we print our name
            builder.AppendLine(type.Name);
            // Print an open brace
            Indent(builder, indent);
            builder.AppendLine("{");

            var childrenFieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var fieldInfo in childrenFieldInfos)
            {
                var child = fieldInfo.GetValue(this);
                if (child is ParseTree.Node) continue;

                Indent(builder, indent + 1);
                builder.Append($"{fieldInfo.Name}: ");
                Dump(builder, child, indent + 1);
            }

            // Print a close brace
            Indent(builder, indent);
            builder.AppendLine("}");
        }

        private static void Dump(StringBuilder builder, object? value, int indent)
        {
            if (ReferenceEquals(value, null))
            {
                builder.AppendLine("null");
            }
            else if (value is Node node)
            {
                node.Dump(builder, indent);
            }
            else if (value is IEnumerable list && !(value is string))
            {
                builder.AppendLine("[");
                foreach (var element in list)
                {
                    Indent(builder, indent + 1);
                    Dump(builder, element, indent + 1);
                }
                Indent(builder, indent);
                builder.AppendLine("]");
            }
            else
            {
                builder.AppendLine(value?.ToString());
            }
        }
    }
}
