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
        /// Void type.
        /// </summary>
        public record Void : Type
        {
            public override string ToString() => "void";
        }
    }
}
