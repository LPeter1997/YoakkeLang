using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Text;

namespace Yoakke.Syntax.ParseTree
{
    /// <summary>
    /// The base class for every parse tree node.
    /// </summary>
    public abstract class Node : IParseTreeElement
    {
        public abstract Span Span { get; }
        public virtual IEnumerable<Token> Tokens
        {
            get
            {
                foreach (var child in Children)
                {
                    if (child is Token t) yield return t;
                    else if (child is Node n)
                    {
                        foreach (var token in child.Tokens) yield return token;
                    }
                    else throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// All of the children of this <see cref="Node"/>.
        /// </summary>
        public virtual IEnumerable<IParseTreeElement> Children
        {
            get
            {
                var fieldInfos = GetRelevantFields(GetType());
                foreach (var fieldInfo in fieldInfos)
                {
                    var value = fieldInfo.GetValue(this);
                    if (ReferenceEquals(value, null)) continue;
                    if (value is IParseTreeElement treeElement)
                    {
                        yield return treeElement;
                    }
                    else if (value is IList list)
                    {
                        foreach (var sub in list) yield return (IParseTreeElement)sub;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
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

            var childrenFieldInfos = GetRelevantFields(type);
            foreach (var fieldInfo in childrenFieldInfos)
            {
                var child = fieldInfo.GetValue(this);

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
                else if (child is IList list)
                {
                    builder.AppendLine("[");
                    foreach (var element in list)
                    {
                        if (element is Node nodeElement)
                        {
                            Indent(builder, indent + 2);
                            nodeElement.Dump(builder, indent + 2);
                        }
                        else if (element is Token token)
                        {
                            Indent(builder, indent + 2);
                            builder.AppendLine(token.Value);
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
                else if (child is Token token)
                {
                    builder.AppendLine(token.Value);
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

        // Field info cache thing

        private static readonly Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();
        private FieldInfo[] GetRelevantFields(Type type)
        {
            if (fieldCache.TryGetValue(type, out var existingFieldInfos))
            {
                return existingFieldInfos;
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var relevantFields = fields.Where(field =>
            {
                if (typeof(IParseTreeElement).IsAssignableFrom(field.FieldType)) return true;
                if (typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                {
                    var genericArgs = field.FieldType.GetGenericArguments();
                    if (genericArgs.Length > 0 && typeof(IParseTreeElement).IsAssignableFrom(genericArgs[0])) return true;
                }
                return false;
            });
            var result = relevantFields.ToArray();
            fieldCache.Add(type, result);
            return result;
        }
    }

    /// <summary>
    /// A <see cref="Node"/> that has an optional trailing comma.
    /// </summary>
    public class WithComma<T> : Node where T : Node
    {
        public override Span Span => new Span(Element.Span, Comma?.Span ?? Element.Span);
        
        /// <summary>
        /// The element.
        /// </summary>
        public readonly T Element;
        /// <summary>
        /// The optional comma.
        /// </summary>
        public readonly Token? Comma;

        public WithComma(T element, Token? comma)
        {
            Element = element;
            Comma = comma;
        }
    }
}
