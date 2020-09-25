using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir.Types
{
    partial class Type
    {
        /// <summary>
        /// User value indicator type.
        /// </summary>
        public class User : Type
        {
            public override string ToString() => "user";
            public override bool Equals(Type? other) => other is User;
            public override int GetHashCode() => HashCode.Combine(typeof(User));
        }
    }
}
