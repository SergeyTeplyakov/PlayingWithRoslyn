using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SampleNuGetAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonPureMethodCallOnReadonlyStruct : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NonPureMethodCallOnReadonlyStruct";
         
        private static readonly string Title = $"[Non-R#] Non-pure method on readonly struct";
        private static readonly string MessageFormat = "[Non-R#] Impure method is called for readonly field of value type.";
        private const string Category = "CodeStyle";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(InvocationHandler, SyntaxKind.InvocationExpression);
        }

        private void InvocationHandler(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax) context.Node;
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;

            if (memberAccess == null) return;
          
            if (IsReadonlyFieldAccessed(context, memberAccess) && 
                IsNonPureMethodWasCalledOnStruct(context, memberAccess))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

        private bool IsNonPureMethodWasCalledOnStruct(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess)
        {
            var identifier = memberAccess.Name as IdentifierNameSyntax;
            if (identifier == null) return false;

            var identifierSymbol = context.SemanticModel.GetSymbolInfo(identifier);
            var methodCallSymbol = identifierSymbol.Symbol as IMethodSymbol;
            if (methodCallSymbol == null) return false;

            return methodCallSymbol.ReceiverType.IsValueType && methodCallSymbol.ReturnsVoid;
        }

        private bool IsReadonlyFieldAccessed(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess)
        {
            var identifier = memberAccess.Expression as IdentifierNameSyntax;
            if (identifier == null) return false;

            var identifierSymbol = context.SemanticModel.GetSymbolInfo(identifier);

            var field = identifierSymbol.Symbol as IFieldSymbol;
            if (field == null) return false;

            return field.IsReadOnly;
        }
    }
}
