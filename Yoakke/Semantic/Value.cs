using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Ast;

namespace Yoakke.Semantic
{
    abstract class Value
    {
        public abstract Type Type { get; }
    }

    class TypeValue : Value
    {
        public override Type Type => Type.Type_;
        public Type Value { get; set; }

        public TypeValue(Type value)
        {
            Value = value;
        }
    }

    class ProcedureValue : Value
    {
        private Type? type;
        public override Type Type
        {
            get
            {
                if (type == null) type = EvaluateType();
                return type;
            }
        }
        public readonly ProcExpression Node;

        public ProcedureValue(ProcExpression node)
        {
            Node = node;
        }

        private Type EvaluateType()
        {
            throw new NotImplementedException();
        }
    }
}
