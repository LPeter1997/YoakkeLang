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

        /// <summary>
        /// Compiles the given IR <see cref="Assembly"/> to another representation, outputting it to a file.
        /// </summary>
        /// <param name="path">The path to output to.</param>
        /// <param name="outputType">The <see cref="OutputType"/> to target.</param>
        /// <param name="extra">Any extra information the specific code-generator needs.</param>
        /// <returns>The return code of the backend compiler.</returns>
        int CompileAndOutput(NamingContext namingContext, string path, OutputType outputType, object[] extra);
    }
}
