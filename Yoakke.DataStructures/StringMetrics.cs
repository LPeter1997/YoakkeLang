using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.DataStructures
{
    /// <summary>
    /// Different string metric implementations.
    /// </summary>
    public static class StringMetrics
    {
        public static int LevenshteinDistance(string s, string t)
        {
            var v0 = new int[t.Length + 1];
            var v1 = new int[t.Length + 1];

            for (int i = 0; i <= t.Length; ++i) v0[i] = i;

            for (int i = 0; i < s.Length; ++i)
            {
                v1[0] = i + 1;

                for (int j = 0; j < t.Length; ++j)
                {
                    var deletionCost = v0[j + 1] + 1;
                    var insertionCost = v1[j] + 1;
                    var substitutionCost = v0[j] + (s[i] == t[j] ? 0 : 1);

                    v1[j + 1] = Math.Min(deletionCost, Math.Min(insertionCost, substitutionCost));
                }

                var tmp = v1;
                v1 = v0;
                v0 = tmp;
            }

            return v0[t.Length];
        }

        public static int OptimalStringAlignmentDistance(string s, string t)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}
