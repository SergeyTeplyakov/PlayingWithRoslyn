using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using RoslynNUnitLight;

namespace SampleNuGetAnalyzer.NUnitTest
{
    [TestFixture]
    public class NoDefaultCtorTests : AnalyzerTestFixture
    {
        private const string DiagnosticId = StructWithNoDefaultContructorAnalyzer.DiagnosticId;

        public NoDefaultCtorTests()
        {
            LanguageName = "C#";
        }

        protected override string LanguageName { get; }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new StructWithNoDefaultContructorAnalyzer();
        }

        [Test]
        public void WarnIfNoDefaultCtorOnStructWithAttribute()
        {
            
            const string code = @"
class NoDefaultConstructorAttribute : System.Attribute {}
[|[NoDefaultConstructor]
struct RelayCommand {}|]
";

            HasDiagnostic(code, DiagnosticId);
        }
    }
}
