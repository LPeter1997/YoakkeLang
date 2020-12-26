using System;

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

        public static int OptimalStringAlignmentDistance(string a, string b)
        {
            var d = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; ++i) d[i, 0] = i;
            for (int j = 0; j <= b.Length; ++j) d[0, j] = j;

            for (int i = 1; i <= a.Length; ++i)
            {
                for (int j = 1; j <= b.Length; ++j)
                {
                    var deletionCost = d[i - 1, j] + 1;
                    var insertionCost = d[i, j - 1] + 1;
                    var substitutionCost = d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1);

                    d[i, j] = Math.Min(deletionCost, Math.Min(insertionCost, substitutionCost));

                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                    {
                        var transpositionCost = d[i - 2, j - 2] + 1;
                        d[i, j] = Math.Min(d[i, j], transpositionCost);
                    }
                }
            }

            return d[a.Length, b.Length];
        }
    }
}
