using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Yoakke.Compiler.Ast;
using Yoakke.Compiler.IR;
using Yoakke.Compiler.IR.Passes;
using Yoakke.Compiler.Semantic;
using Yoakke.Compiler.Syntax;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args) =>
            CommandLineApplication.Execute<Compiler>(args);
    }
}
