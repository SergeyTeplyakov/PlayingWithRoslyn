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
    public class DoNotUseDefaultCtorTests : AnalyzerTestFixture
    {
        private const string DiagnosticId = DoNotUseDefaultContructorAnalyzer.DiagnosticId;

        public DoNotUseDefaultCtorTests()
        {
            LanguageName = "C#";
        }

        protected override string LanguageName { get; }

        protected override DiagnosticAnalyzer CreateAnalyzer()
        {
            return new DoNotUseDefaultContructorAnalyzer();
        }

        [Test]
        public void DefaultConstructorShouldFail()
        {
            const string code = @"
class NoDefaultConstructorAttribute : System.Attribute {}
[NoDefaultConstructor]
struct RelayCommand {}
class Test {
  static void Run() {
    var command = [|new RelayCommand()|];
  }
}";

            HasDiagnostic(code, DiagnosticId);
        }
        
        [Test]
        public void GenericMethodWithNewKeywordShouldFail()
        {
            const string code = @"
class NoDefaultConstructorAttribute : System.Attribute {}
[NoDefaultConstructor]
struct RelayCommand {}

class Sample {
  static T Create<T>() where T : new() { return new T();}
  static void Test()
  {
    var t = [|Create<RelayCommand>|]();
  }
}";

            HasDiagnostic(code, DiagnosticId);
        }

        [Test]
        public void GenericActivatorCreateInstanceShouldFail()
        {
            const string code = @"
class NoDefaultConstructorAttribute : System.Attribute {}
[NoDefaultConstructor]
struct RelayCommand {}
class Sample {
  static void Test()
  {
    var t = System.Activator.[|CreateInstance<RelayCommand>|]();
  }
}";

            HasDiagnostic(code, DiagnosticId);
        }

        [Test]
        public void GenericClassWithInstanceMethodShouldFail()
        {
            const string code = @"
class NoDefaultConstructorAttribute : System.Attribute {}
[NoDefaultConstructor]
struct RelayCommand {}
class GenericCreator<T> where T : new() {
		public T Create() {
			return new T();
		}
	}
class Sample {
  public static void Test()
  {
    var t = new [|GenericCreator<RelayCommand>|]().Create();
  }
}";

            HasDiagnostic(code, DiagnosticId);
        }

        [Test]
        public void GenericStructWithInstanceMethodShouldFail()
        {
            const string code = @"
class NoDefaultConstructorAttribute : System.Attribute {}
[NoDefaultConstructor]
struct RelayCommand {}
struct GenericCreator<T> where T : new() {
		public T Create() {
			return new T();
		}
	}
class Sample {
  public static void Test()
  {
    var t = new [|GenericCreator<RelayCommand>|]().Create();
  }
}";

            HasDiagnostic(code, DiagnosticId);
        }

    }
}
