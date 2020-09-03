using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Types
{
    partial record Type
    {
        /// <summary>
        /// Pointer type.
        /// </summary>
        public record Ptr(Type Subtype) : Type
        {
            public override string ToString() => $"{Subtype}*";
        }
    }
}
