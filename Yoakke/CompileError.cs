using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke
{
    abstract class CompileError : Exception
    {
        public abstract void Show();
    }
}
