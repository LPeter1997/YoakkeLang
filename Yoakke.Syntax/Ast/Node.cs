using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        private void Dump(StringBuilder builder, int indent)
        {
            void Indent(StringBuilder builder, int amount) => builder.Append(new string(' ', amount * 2));

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
                if (ReferenceEquals(child, null))
                {
                    builder.AppendLine("null");
                }
                else if (child is Node node)
                {
                    node.Dump(builder, indent + 1);
                }
                else if (child is IEnumerable list)
                {
                    builder.AppendLine("[");
                    foreach (var element in list)
                    {
                        if (element is Node nodeElement)
                        {
                            Indent(builder, indent + 2);
                            nodeElement.Dump(builder, indent + 2);
                        }
                        else
                        {
                            Indent(builder, indent + 2);
                            builder.AppendLine(element?.ToString());
                        }
                    }
                    Indent(builder, indent + 1);
                    builder.AppendLine("]");
                }
                else
                {
                    builder.AppendLine(child?.ToString());
                }
            }

            // Print a close brace
            Indent(builder, indent);
            builder.AppendLine("}");
        }
    }
}
