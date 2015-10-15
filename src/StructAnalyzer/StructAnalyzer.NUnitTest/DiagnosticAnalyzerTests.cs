//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;
//using Microsoft.CodeAnalysis.Text;
//using NUnit.Framework;
//using RoslynNUnitLight;

//namespace SampleNuGetAnalyzer.NUnitTest
//{
//    [TestFixture]
//    public class DiagnosticAnalyzerTests : AnalyzerTestFixture
//    {
//        public DiagnosticAnalyzerTests()
//        {
//            LanguageName = "C#";
//        }

//        protected override string LanguageName { get; }

//        protected override DiagnosticAnalyzer CreateAnalyzer()
//        {
//            return new RelayCommandDefaultContructorAnalyzer();
//        }

//        [Test]
//        public void DefaultConstructorShouldFail()
//        {
//            const string code = @"
//var command = [|new RelayCommand()|];";

//            HasDiagnostic(code, RelayCommandDefaultContructorAnalyzer.DiagnosticId);
//        }

//        [Test]
//        public void GenericMethodWithNewKeywordShouldFail()
//        {
//            const string code = @"
//struct RelayCommand {}
//class Sample {
//  static T Create<T>() where T : new() { return new T();}
//  static void Test()
//  {
//    var t = [|Create<RelayCommand>|]();
//  }
//}";

//            HasDiagnostic(code, RelayCommandDefaultContructorAnalyzer.DiagnosticId);
//        }

//        [Test]
//        public void GenericActivatorCreateInstanceShouldFail()
//        {
//            const string code = @"
//struct RelayCommand {}
//class Sample {
//  static void Test()
//  {
//    var t = System.Activator.[|CreateInstance<RelayCommand>|]();
//  }
//}";

//            HasDiagnostic(code, RelayCommandDefaultContructorAnalyzer.DiagnosticId);
//        }

//        [Test]
//        public void GenericClassWithInstanceMethodShouldFail()
//        {
//            const string code = @"
//struct RelayCommand {}
//class GenericCreator<T> where T : new() {
//		public T Create() {
//			return new T();
//		}
//	}
//class Sample {
//  public static void Test()
//  {
//    var t = [|new GenericCreator<RelayCommand>()|].Create();
//  }
//}";

//            HasDiagnostic(code, RelayCommandDefaultContructorAnalyzer.DiagnosticId);
//        }

//    }
//}
