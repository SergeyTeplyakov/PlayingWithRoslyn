using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using MvvmUltraLight.Analyzer;

namespace MvvmUltraLight.Analyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
[TestMethod]
public void ShouldBeWarningOnDefaultConstructor()
{
    var test = @"
namespace MvvmUltraLight.Core {
  public struct RelayCommand {
    public RelayCommand(Action action) {}
  }
}
namespace ConsoleApplication1 {
  using MvvmUltraLight.Core;
  class TypeName {
    public static void Run() {
     var r = new RelayCommand();
    }   
  }
}";
    var expected = new DiagnosticResult
    {
        Id = DoNotUseDefaultCtorAnalyzer.DiagnosticId,
        Message = DoNotUseDefaultCtorAnalyzer.MessageFormat,
        Severity = DiagnosticSeverity.Warning,
        Locations = new[] {new DiagnosticResultLocation("Test0.cs", 11, 14) }
    };

    VerifyCSharpDiagnostic(test, expected);
}

[TestMethod]
public void ShouldBeWarningOnGenericFacotry()
{
            var test = @"
namespace MvvmUltraLight.Core {
  public struct RelayCommand {
    public RelayCommand(Action action) {}
  }
}
namespace ConsoleApplication1 {
  using MvvmUltraLight.Core;
  class TypeName {
    public static T Create<T>() where T: new() {return new T();}
    public static void Run() {
     var r = Create<RelayCommand>();
    }   
  }
}";
    var expected = new DiagnosticResult
    {
        Id = DoNotUseDefaultCtorAnalyzer.DiagnosticId,
        Message = DoNotUseDefaultCtorAnalyzer.MessageFormat,
        Severity = DiagnosticSeverity.Warning,
        Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 14) }
    };

    VerifyCSharpDiagnostic(test, expected);
}

        [TestMethod]
        public void ShouldBeWarningOnActivatorCreateInstance()
        {
            var test = @"
namespace MvvmUltraLight.Core {
  public struct RelayCommand {
    public RelayCommand(Action action) {}
  }
}
namespace ConsoleApplication1 {
  using MvvmUltraLight.Core;
  class TypeName {
    public static void Run() {
     var r = System.Activator.CreateInstance<RelayCommand>();
    }   
  }
}";
            var expected = new DiagnosticResult
            {
                Id = DoNotUseDefaultCtorAnalyzer.DiagnosticId,
                Message = DoNotUseDefaultCtorAnalyzer.MessageFormat,
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 12, 14) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
public void FixShouldAddLambdaToConstructor()
{
    var code = @"
namespace ConsoleApplication1 {
    class TypeName {
        public static void Run() {
                var r = new MvvmUltraLight.Core.RelayCommand();
        }   
    }
}";
    var fixedCode = @"
namespace ConsoleApplication1 {
    class TypeName {
        public static void Run() {
                var r = new MvvmUltraLight.Core.RelayCommand(() => {});
        }   
    }
}";

    VerifyCSharpFix(code, fixedCode);
}

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseNonDefaultCtorCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DoNotUseDefaultCtorAnalyzer();
        }
    }
}