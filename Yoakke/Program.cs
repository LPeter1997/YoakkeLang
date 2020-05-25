using System;
using Yoakke.Syntax;

namespace Yoakke
{
    class Program
    {
        static void Main(string[] args)
        {
            var src = new Source(@"
    const Person = struct(T: type) {
        name: *char,
        age: i32,
    }

    const main = proc() -> i32 {
        0
    }
");
            Console.WriteLine(Annotation.Annotate(src, new Position(line: 2, column: 14)));
        }
    }
}
