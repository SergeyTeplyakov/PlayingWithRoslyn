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
    public class RelayCommandDefaultContructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DoNotUseRelayCommandDefaultCtor";

        private static readonly string Title = "Avoid using default constructor for this struct";
        private static readonly string GenericTitle = "Do not use 'RelayCommand' with generic methods with new() constraint";
        private static readonly string MessageFormat = "Do not use default constructor for 'RelayCommand' struct";
        private const string Category = "CodeStyle";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor GenericContraintRule = new DiagnosticDescriptor(
            DiagnosticId, GenericTitle, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, GenericContraintRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(ObjectCreationHandler, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(GenericNameHandler, SyntaxKind.GenericName);
        }

        private void GenericNameHandler(SyntaxNodeAnalysisContext context)
        {
            var genericNameSyntax = (GenericNameSyntax)context.Node;
            // There is different cases here
            // 1. Static generic method
            var relaySymbol = genericNameSyntax?.TypeArgumentList.Arguments
                .OfType<IdentifierNameSyntax>()
                .Select(identifier => context.SemanticModel.GetSymbolInfo(identifier))
                .FirstOrDefault(symbol => symbol.Symbol != null && symbol.Symbol.Name.ToLower() == "relaycommand");

            if (relaySymbol != null)
            {
                var genericMethodSymbol = context.SemanticModel.GetSymbolInfo(genericNameSyntax).Symbol as IMethodSymbol;
                var activatorType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Activator).FullName);

                // Activator.CreateInstance<T> is a special case, because this method does not have new() constraint!
                bool isActivator = genericMethodSymbol?.ContainingType.Equals(activatorType) == true && genericMethodSymbol?.IsGenericMethod == true;

                if (isActivator || genericMethodSymbol?.TypeParameters.Any(tp => tp.HasConstructorConstraint) == true)
                {
                    context.ReportDiagnostic(Diagnostic.Create(GenericContraintRule, context.Node.GetLocation()));
                }
            }
        }

        private void ObjectCreationHandler(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            if (objectCreation.Type.GetText().ToString().ToLower().Contains("relaycommand") && objectCreation.ArgumentList.Arguments.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}
