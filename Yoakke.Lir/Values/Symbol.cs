﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir.Types;

namespace Yoakke.Lir.Values
{
    partial record Value
    {
        /// <summary>
        /// Reference to an <see cref="ISymbol"/>.
        /// </summary>
        public record Symbol(ISymbol Value) : Value
        {
            public override Type Type => Value.Type;

            public override string ToString() => Value.Name;
        }
    }
}
