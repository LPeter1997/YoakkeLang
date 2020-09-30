using System.Collections.Generic;
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
        public static readonly IReadOnlyList<ICodePass> DefaultPasses = new ICodePass[]
        {
            new GroupAllocs(),
        };

        /// <summary>
        /// The <see cref="ICodePass"/>es to perform on an <see cref="Assembly"/>.
        /// </summary>
        public readonly IList<ICodePass> Passes = new List<ICodePass>();

        /// <summary>
        /// Initializes a new <see cref="CodePassSet"/> with the default <see cref="ICodePass"/>es.
        /// See the <see cref="DefaultPasses"/>.
        /// </summary>
        public CodePassSet()
        {
            foreach (var p in DefaultPasses) Passes.Add(p);
        }

        /// <summary>
        /// Applies the contained <see cref="Passes"/> as long as there are changes found.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> to apply the passes on.</param>
        /// <param name="changed">Outputs true, if the code was changed.</param>
        public void Pass(Assembly assembly, out bool changed)
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
        public void Pass(Assembly assembly) => Pass(assembly, out var _);

        private bool Pass(Assembly assembly, bool first)
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
