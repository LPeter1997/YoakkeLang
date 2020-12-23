using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Yoakke.Reporting.Info;
using Yoakke.Reporting.Render;
using Yoakke.Text;

namespace Yoakke.Reporting.Tests
{
    [TestClass]
    public class ConsoleRenderingTests
    {
        [TestMethod]
        public void BasicSingleAnnotation()
        {
            var src = new SourceFile("simple.txt",
@"line 1
prev line
this is a line of text
next line
some other line");
            var diag = new Diagnostic 
            { 
                Severity = Severity.Error,
                Code = "E0001",
                Message = "Some error message",
                Information =
                {
                    new PrimaryDiagnosticInfo
                    {
                        Span = new Span(src, new Position(line: 2, column: 10), 4),
                        Message = "some annotation",
                    }
                },
            };
            var result = new StringWriter();
            var renderer = new TextDiagnosticRenderer(result);
            renderer.Render(diag);
            Assert.AreEqual(@"error[E0001]: Some error message
  ┌─ simple.txt
  │
2 │ prev line
3 │ this is a line of text
  │           ^^^^ some annotation
4 │ next line
  │
", result.ToString());
        }
    }
}
