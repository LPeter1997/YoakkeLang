using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    /// <summary>
    /// An <see cref="Intrinsic"/> for importing other files.
    /// </summary>
    public class ImportIntrinsic : Intrinsic
    {
        public override Lir.Types.Type ReturnType { get; }
        public override Type Type { get; }

        public ImportIntrinsic(IDependencySystem system)
            : base(system)
        {
            var stringType = System.ReferToConstType("@c", "str");
            Type = new Type.Proc(new ValueList<Type> { stringType }, Type.Type_);
            ReturnType = Lir.Types.Type.User_;
        }

        public override Value Execute(VirtualMachine vm, IEnumerable<Value> args)
        {
            throw new NotImplementedException();
        }
    }
}
