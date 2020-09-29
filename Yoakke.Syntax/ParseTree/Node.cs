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
                if (typeof(IList).IsAssignableFrom(field.FieldType))
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
