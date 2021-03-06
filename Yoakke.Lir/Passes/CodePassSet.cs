﻿using System.Collections.Generic;
using System.Linq;

namespace Yoakke.Lir.Passes
{
    /// <summary>
    /// A set of <see cref="ICodePass"/>es to apply to an <see cref="Assembly"/>.
    /// </summary>
    public class CodePassSet : ICodePass
    {
        public bool IsSinglePass => Passes.All(p => p.IsSinglePass);

        /// <summary>
        /// A collection of default <see cref="ICodePass"/>es.
        /// </summary>
        public static readonly ICodePass BasicPass = new CodePassSet
        {
            Passes =
            { 
                new GroupAllocs(),
            }
        };

        /// <summary>
        /// The <see cref="ICodePass"/>es to perform on an <see cref="Assembly"/>.
        /// </summary>
        public readonly IList<ICodePass> Passes = new List<ICodePass>();

        /// <summary>
        /// Initializes a new, empty <see cref="CodePassSet"/>.
        /// </summary>
        public CodePassSet()
        {
        }

        /// <summary>
        /// Applies the contained <see cref="Passes"/> as long as there are changes found.
        /// </summary>
        /// <param name="assembly">The <see cref="UncheckedAssembly"/> to apply the passes on.</param>
        /// <param name="changed">Outputs true, if the code was changed.</param>
        public void Pass(UncheckedAssembly assembly, out bool changed)
        {
            changed = false;
            bool first = true;
            while (true)
            {
                if (!Pass(assembly, first)) break;
                changed = true;
                first = false;
            }
        }

        /// <summary>
        /// Same as <see cref="Pass(Assembly, out bool)"/>.
        /// </summary>
        public void Pass(UncheckedAssembly assembly) => Pass(assembly, out var _);

        private bool Pass(UncheckedAssembly assembly, bool first)
        {
            bool changed = false;
            foreach (var pass in Passes)
            {
                if ((pass.IsSinglePass && first) || !pass.IsSinglePass)
                {
                    pass.Pass(assembly, out var passChanged);
                    changed = changed || passChanged;
                }
            }
            return changed;
        }
    }
}
