using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    /// <summary>
    /// An <see cref="IIntrinsic"/> that imports an external symbol.
    /// </summary>
    public class ExternIntrinsic : IIntrinsic
    {
        private Dictionary<string, (Extern, Type)> externals = new Dictionary<string, (Extern, Type)>();

        public Value Evaluate(IDependencySystem system, IList<Expression> args)
        {
            var stringType = system.ReferToConstType("@c", "str");
            // TODO: Proper errors and such
            Debug.Assert(args.Count == 2);
            Debug.Assert(system.TypeOf(args[0]).Equals(Type.Type_));
            Debug.Assert(system.TypeOf(args[1]).Equals(stringType));
            // Now we evaluate the arguments
            // First the type
            var type = system.EvaluateType(args[0]);
            // Then the name
            var nameSlice = (Value.Struct)system.Evaluate(args[0]);
            var namePtr = nameSlice.Values[0];
            var nameLen = nameSlice.Values[1];
            // TODO
            throw new NotImplementedException();
        }
    }
}
