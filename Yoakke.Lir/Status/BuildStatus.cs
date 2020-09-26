using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Status
{
    /// <summary>
    /// A place to report errors and warnings for the build.
    /// </summary>
    public class BuildStatus
    {
        /// <summary>
        /// The warnings reported so far.
        /// </summary>
        public readonly IList<IBuildWarning> Warnings = new List<IBuildWarning>();
        /// <summary>
        /// The errors reported so far.
        /// </summary>
        public readonly IList<IBuildError> Errors = new List<IBuildError>();

        public void Report(IBuildWarning warning) => Warnings.Add(warning);
        public void Report(IBuildError error) => Errors.Add(error);
    }

    /// <summary>
    /// Interface for warnings.
    /// </summary>
    public interface IBuildWarning { }

    /// <summary>
    /// Interface for errors.
    /// </summary>
    public interface IBuildError { }
}
