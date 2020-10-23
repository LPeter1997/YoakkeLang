using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.DataStructures;
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
        public override Type Type { get; }

        public ImportIntrinsic(IDependencySystem system)
            : base(system)
        {
            var stringType = System.ReferToConstType("@c", "str");
            Type = new Type.Proc(new ValueList<Type> { stringType }, Type.Type_);
        }

        public override Value Evaluate(IList<Expression> args)
        {
            throw new NotImplementedException();
        }
    }
}
