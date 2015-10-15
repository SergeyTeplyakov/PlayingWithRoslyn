using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
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
