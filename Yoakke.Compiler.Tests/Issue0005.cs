namespace Yoakke.Compiler.Tests
{
    // https://github.com/LPeter1997/YoakkeLang/issues/5

    [TestClass]
    public class Issue0005 : TestBase
    {
        [TestMethod]
        public void Test()
        {
            string source = @"
const bar = proc(x: var) { };

const main = proc() -> i32 {
    bar(3);
    bar(true);
    0
};
";
            Assert.AreEqual(Compile(source), 0);
        }

        [TestMethod]
        public void TestIdentity()
        {
            string source = @"
const identity = proc(x: var) -> var { x };

const main = proc() -> i32 {
    var a: i32 = identity(3);
    var b: bool = identity(true);
    0
};
";
            Assert.AreEqual(Compile(source), 0);
        }
    }
}
