using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yoakke.Syntax.Ast;

namespace Yoakke.Compiler.Semantic
{
    // TODO: Doc
    public class NameContext : Visitor<object?>
    {
        private Dictionary<object, string> assignedNames = new Dictionary<object, string>();
        private HashSet<string> allocatedNames = new HashSet<string>();
        private Stack<string> currentName = new Stack<string>();

        public void NameAll(Declaration.File file) => Visit(file);

        public string NameOf(object obj)
        {
            if (assignedNames.TryGetValue(obj, out var value)) return value;
            return AllocateName(obj, "unnamed");
        }

        protected override object? Visit(Declaration.Const cons)
        {
            AllocateName(cons.Value, Push(cons.Name));
            base.Visit(cons);
            currentName.Pop();
            return null;
        }

        protected override object? Visit(Statement.Var var)
        {
            AllocateName(var, Push(var.Name));
            base.Visit(var);
            currentName.Pop();
            return null;
        }

        private string Push(string part)
        {
            currentName.Push(part);
            return ComposeName();
        }

        private string ComposeName()
        {
            Debug.Assert(currentName.Count > 0);
            return string.Join('.', currentName.Reverse());
        }

        private string AllocateName(object obj, string suggestion)
        {
            if (TryAllocateName(obj, suggestion)) return suggestion;
            for (int cnt = 0; ; ++cnt)
            {
                var nextSuggestion = $"{suggestion}_{cnt}";
                if (TryAllocateName(obj, nextSuggestion)) return nextSuggestion;
            }
        }

        private bool TryAllocateName(object obj, string name)
        {
            if (!allocatedNames.Contains(name))
            {
                assignedNames[obj] = name;
                allocatedNames.Add(name);
                return true;
            }
            return false;
        }
    }
}
