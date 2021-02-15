using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Dependency
{
#pragma warning disable CS0660, CS0661
    internal struct Revision
#pragma warning restore CS0660, CS0661
    {
        private int counter;

        public Revision(int counter)
        {
            this.counter = counter;
        }

        public void Increment() => ++counter;

        public override string ToString() => $"R{counter}";

        public static bool operator ==(Revision r1, Revision r2) => r1.counter == r2.counter;
        public static bool operator !=(Revision r1, Revision r2) => r1.counter != r2.counter;
        public static bool operator <(Revision r1, Revision r2) => r1.counter < r2.counter;
        public static bool operator >(Revision r1, Revision r2) => r1.counter > r2.counter;
        public static bool operator <=(Revision r1, Revision r2) => r1.counter <= r2.counter;
        public static bool operator >=(Revision r1, Revision r2) => r1.counter >= r2.counter;
    }
}
