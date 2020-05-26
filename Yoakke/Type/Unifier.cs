using System;
using System.Collections.Generic;
using System.Text;

namespace Yoakke.Type
{
    static class Unifier
    {
        public static void Unify(Type t1, Type t2)
        {
            t1 = t1.Substitution;
            t2 = t2.Substitution;
            if (t1 is TypeVariable v1)
            {
                if (t2 is TypeVariable v2)
                {
                    // Substitute type variable for another
                    if (ReferenceEquals(v1, v2)) return;
                    v2.SubstituteFor(v1);
                    return;
                }
                // Substitute type variable for something else
                if (t2.Contains(v1))
                {
                    throw new NotImplementedException("Type-recursion!");
                }
                v1.SubstituteFor(t2);
                return;
            }
            // TODO
        }
    }
}
