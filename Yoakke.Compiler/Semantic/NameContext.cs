using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    // TODO: Doc
    public class NameContext : Visitor<object?>
    {
        public IReadOnlyDictionary<Node, string> Names => names;

        private Dictionary<Node, string> names = new Dictionary<Node, string>();
        private Stack<string> currentName = new Stack<string>();

        public void NameAll(Declaration.File file) => Visit(file);

        protected override object? Visit(Declaration.Const cons)
        {
            currentName.Push(cons.Name);
            names[cons.Value] = ComposeName();
            base.Visit(cons);
            currentName.Pop();
            return null;
        }

        private string ComposeName()
        {
            Debug.Assert(currentName.Count > 0);
            return string.Join('.', currentName);
        }
    }
}
