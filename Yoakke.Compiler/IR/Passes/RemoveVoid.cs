using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.IR;

namespace Yoakke.Compiler.IR.Passes
{
    class RemoveVoid : IPass
    {
        public bool Pass(Assembly assembly)
        {
            bool hasChanged = false;
            foreach (var proc in assembly.Procedures)
            {
                Pass(proc, out bool procChanged);
                hasChanged = hasChanged || procChanged;
            }
            return hasChanged;
        }

        private void Pass(Proc proc, out bool changed)
        {
            // TODO
            throw new NotImplementedException();
        }

        private void Pass(BasicBlock bb, out bool changed)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
