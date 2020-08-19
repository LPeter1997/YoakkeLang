using System.Collections.Generic;

namespace Yoakke.Lir
{
    /// <summary>
    /// A compilation unit for the IR code.
    /// </summary>
    public class Assembly
    {
        /// <summary>
        /// The entry point of the <see cref="Assembly"/>.
        /// If null, the procedure named "main" will be chosen, or the singleton, if there's 
        /// only one procedure defined.
        /// </summary>
        public Proc? EntryPoint { get; set; }

        /// <summary>
        /// The <see cref="Proc"/>s defined in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IList<Proc> Procedures = new List<Proc>();

        public override string ToString() =>
            string.Join("\n\n", Procedures).Trim();
    }
}
