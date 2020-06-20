﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yoakke.Compiler.Utils
{
    /// <summary>
    /// Command line utilities.
    /// </summary>
    static class CommandLine
    {
        /// <summary>
        /// Escapes the given arguments so they can be passed safely to a process.
        /// </summary>
        /// <param name="args">The list of arguments.</param>
        /// <returns>The escaped argument list as a string.</returns>
        public static string EscapeArgs(IEnumerable<object> args) =>
            args.Select(x => $"\"{x}\"").StringJoin(" ");
    }
}