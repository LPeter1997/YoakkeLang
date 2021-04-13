using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Symbols.Impl
{
    partial class Scope
    {
        public class Local : Scope
        {
            public Local(IScope parent)
                : base(parent)
            {
            }
        }
    }
}
