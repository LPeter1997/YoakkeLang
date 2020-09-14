﻿using Yoakke.DataStructures;

namespace Yoakke.Lir.Types
{
    partial record Type
    {
        /// <summary>
        /// Procedure type.
        /// </summary>
        public record Proc(CallConv CallConv, Type Return, IValueList<Type> Parameters) : Type
        {
            public override string ToString() => 
                $"{Return} proc[callconv = {CallConv}]({string.Join(", ", Parameters)})";
        }
    }
}
