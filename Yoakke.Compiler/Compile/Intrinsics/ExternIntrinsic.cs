using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Lir;
using Yoakke.Lir.Values;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    /// <summary>
    /// An <see cref="IIntrinsic"/> that imports an external symbol.
    /// </summary>
    public class ExternIntrinsic : IIntrinsic
    {
        private Dictionary<string, Extern> externals = new Dictionary<string, Extern>();

        public Value Evaluate(IDependencySystem system, IList<Value> args)
        {
            // TODO: We need strings!
            throw new NotImplementedException();
        }
    }
}
