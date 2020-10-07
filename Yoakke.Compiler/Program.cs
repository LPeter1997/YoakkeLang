﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Yoakke.C.Syntax.Cpp;
using Yoakke.Compiler.Compile;
using Yoakke.Compiler.Semantic;
using Yoakke.DataStructures;
using Yoakke.Lir.Backend;
using Yoakke.Lir.Backend.Toolchain;
using Yoakke.Lir.Passes;
using Yoakke.Lir.Runtime;
using Yoakke.Lir.Status;
using Yoakke.Lir.Values;
using Yoakke.Syntax;
using Yoakke.Syntax.Ast;
using Yoakke.Text;

namespace Yoakke.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = @"
hell??/   
o world??(
";
            var reader = new CppTextReader(new StringReader(src));
            while (true)
            {
                char? ch = reader.Next();
                if (ch == null) break;
                Console.Write(ch.Value);
            }
        }
    }
}
