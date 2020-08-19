using System.Collections.Generic;

namespace Yoakke.Lir
{
    /// <summary>
    /// A compilation unit for the IR code.
    /// </summary>
    public class Assembly
    {
        // TODO: Entry point name?
        /// <summary>
        /// The <see cref="Proc"/>s defined in this <see cref="Assembly"/>.
        /// </summary>
        public readonly IList<Proc> Procedures = new List<Proc>();

        public override string ToString() =>
            string.Join("\n\n", Procedures).Trim();
    }
}
