using System;
using System.Linq;
using Yoakke.DataStructures;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// Procedure type.
        /// </summary>
        public class Proc : Type
        {
            public readonly CallConv CallConv;
            public readonly Type Return;
            public readonly IValueList<Type> Parameters;

            public Proc(CallConv callConv, Type ret, IValueList<Type> parameters)
            {
                CallConv = callConv;
                Return = ret;
                Parameters = parameters;
            }

            public override string ToTypeString() => 
                $"{Return} proc[callconv = {CallConv}]({string.Join(", ", Parameters.Select(p => p.ToTypeString()))})";
            public override bool Equals(Type? other) =>
                   other is Proc p 
                && CallConv == p.CallConv && Return.Equals(p.Return) && Parameters.Equals(p.Parameters);
            public override int GetHashCode() => HashCode.Combine(typeof(Proc), CallConv, Return, Parameters);
        }
    }
}
