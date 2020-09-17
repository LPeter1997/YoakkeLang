﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Backend
{
    /// <summary>
    /// All information required for a <see cref="IToolchain"/> to perform a build.
    /// </summary>
    public class Build
    {
        /// <summary>
        /// The <see cref="TargetTriplet"/> the build targets.
        /// </summary>
        public TargetTriplet TargetTriplet { get; set; }
        /// <summary>
        /// The <see cref="OutputKind"/> the build needs to produce.
        /// </summary>
        public OutputKind OutputKind { get; set; } = OutputKind.Executable;
        /// <summary>
        /// The directory the intermediate files should be stored in.
        /// </summary>
        public string IntermediatesDirectory { get; set; } = ".";
        /// <summary>
        /// The path of the output file.
        /// </summary>
        public string OutputPath { get; set; } = "a.out";
        /// <summary>
        /// The <see cref="Assembly"/>s that need to be compiled.
        /// </summary>
        public IList<Assembly> Assemblies { get; } = new List<Assembly>();
        /// <summary>
        /// The <see cref="ISymbol"/>s that needs to be exported.
        /// </summary>
        public ISet<ISymbol> Exports { get; } = new HashSet<ISymbol>();
        /// <summary>
        /// The entry point's name.
        /// </summary>
        public string EntryPoint { get; set; } = "main";
        /// <summary>
        /// Any extra information the build needs to carry.
        /// </summary>
        public IDictionary<string, object> Extra { get; } = new Dictionary<string, object>();
    }
}
