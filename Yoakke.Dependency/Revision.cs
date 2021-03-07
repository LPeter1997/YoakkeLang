using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
    /// <summary>
    /// Represents a revision version.
    /// </summary>
    public readonly struct Revision : IEquatable<Revision>
    {
        public static readonly Revision Invalid = new Revision(-1);
        public static readonly Revision MaxValue = new Revision(int.MaxValue);

        private readonly int value;

        public Revision(int value)
        {
            this.value = value;
        }

        internal Revision Next() => new Revision(value + 1);

        public Revision Before(int n) => new Revision(value - n);

        public override bool Equals(object obj) => obj is Revision r && Equals(r);
        public bool Equals(Revision other) => value == other.value;
        public override int GetHashCode() => value.GetHashCode();

        public override string ToString() => value < 0 ? "<none>" : $"R{value}";

        public static bool operator ==(Revision r1, Revision r2) => r1.value == r2.value;
        public static bool operator !=(Revision r1, Revision r2) => r1.value != r2.value;
        public static bool operator >(Revision r1, Revision r2) => r1.value > r2.value;
        public static bool operator <(Revision r1, Revision r2) => r1.value < r2.value;
        public static bool operator >=(Revision r1, Revision r2) => r1.value >= r2.value;
        public static bool operator <=(Revision r1, Revision r2) => r1.value <= r2.value;
    }
}
