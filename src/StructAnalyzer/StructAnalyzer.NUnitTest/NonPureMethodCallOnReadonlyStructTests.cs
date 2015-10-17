using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using RoslynNUnitLight;

namespace SampleNuGetAnalyzer.NUnitTest
{
    [TestFixture]
    public class NonPureMethodCallOnReadonlyStructTests : AnalyzerTestFixture
    {
        private const string DiagnosticId = NonPureMethodCallOnReadonlyStruct.DiagnosticId;

        protected override string LanguageName { get; } = "C#";

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new NonPureMethodCallOnReadonlyStruct();
        }

[Test]
public void WarnForNonPureMethodCallsOnReadonlyStructs()
{   
            const string code = @"
struct Mutable
{
	public int X { get; private set; }

	public void Increment() { X++; }
}

class Sample
{
	private readonly Mutable _mutable = new Mutable();

	public void Foo()
	{
		// Impure method is called for readonly field of value type.
		[|_mutable.Increment()|];
	}
}";

    HasDiagnostic(code, DiagnosticId);
}
    }
}
