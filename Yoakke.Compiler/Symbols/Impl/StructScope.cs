using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Compiler.Symbols.Impl
{
    partial class Scope
    {
        public class Struct : Scope
        {
            public Struct(IScope? parent) 
                : base(parent)
            {
            }
        }
    }
}
