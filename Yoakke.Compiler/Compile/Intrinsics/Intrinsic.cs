using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoakke.Compiler.Semantic;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Types;
using Yoakke.Lir.Values;
using Yoakke.Syntax.Ast;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler.Compile.Intrinsics
{
    /// <summary>
    /// Base for compiler intrinsic values.
    /// </summary>
    public abstract class Intrinsic : IUserProc
    {
        public abstract Lir.Types.Type ReturnType { get; }

        /// <summary>
        /// The <see cref="IDependencySystem"/>.
        /// </summary>
        public IDependencySystem System { get; set; }
        /// <summary>
        /// The semantic <see cref="Type"/> of this <see cref="Intrinsic"/>.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Initiealizes a new <see cref="Intrinsic"/>.
        /// </summary>
        /// <param name="system">The <see cref="IDependencySystem"/>.</param>
        public Intrinsic(IDependencySystem system)
        {
            System = system;
        }

        public abstract Value Execute(VirtualMachine vm, IEnumerable<Value> args);
    }
}
