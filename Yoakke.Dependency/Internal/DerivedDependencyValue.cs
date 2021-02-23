using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency.Internal
{
    /// <summary>
    /// A value that came from a computation.
    /// </summary>
    public class DerivedDependencyValue : IDependencyValue
    {
        public int ChangedAt => throw new NotImplementedException();

        public int VerifiedAt => throw new NotImplementedException();

        public bool NeedsRecomputing => throw new NotImplementedException();

        public T GetValue<T>(DependencySystem system, [CallerMemberName] string memberName = "")
        {
            throw new NotImplementedException();
        }
    }
}
