﻿using McMaster.Extensions.CommandLineUtils;
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
using Yoakke.Compiler.Utils;
using Type = Yoakke.Compiler.Semantic.Type;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            /*if (Debugger.IsAttached && args.Length == 0)
            {
                // For simplicity we inject parameters so we can run from the IDE
                var cmp = new Compiler
                {
                    SourceFile = "../../../../../../samples/test.yk",
                    //DumpAst = true,
                    DumpIr = true,
                    ExecuteImmediately = true,
                    OptimizationLevel = 0,
                    BackendFlags = new string[] { "../../../../../../samples/ffi.c" },
                };
                cmp.OnExecute();
            }
            else
            {
                CommandLineApplication.Execute<Compiler>(args);
            }*/
            var d1 = new Dictionary<string, int> { { "asd", 45 }, { "def", 77 } };
            var d2 = new Dictionary<string, int> { { "def", 77 }, { "asd", 45 } };
            Console.WriteLine(d1.Equals(d2));
        }
    }
}
