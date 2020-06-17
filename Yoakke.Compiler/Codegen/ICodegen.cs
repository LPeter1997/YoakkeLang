using System;
using System.Collections.Generic;
using System.Text;
using Yoakke.Compiler.IR;

namespace Yoakke.Compiler.Codegen
{
    /// <summary>
    /// The interface for every code-generation backend that compiles the IR into some other representation.
    /// </summary>
    interface ICodegen
    {
        /// <summary>
        /// Compiles the given IR <see cref="Assembly"/> to another representation.
        /// </summary>
        /// <param name="namingContext">The <see cref="NamingContext"/> of the <see cref="Assembly"/> to compile.</param>
        /// <returns>Another representation of the IR code.</returns>
        string Compile(NamingContext namingContext);
    }
}
