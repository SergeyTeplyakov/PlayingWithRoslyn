using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MvvmUltraLight.Core;

namespace MvvmUltraLight.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseDefaultCtorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DoNotUseRelayCommandDefaultCtor";

        private static readonly string Title = "Default constructor for 'RelayCommand' considered harmful";
        public static readonly string MessageFormat = "Do not use default constructor for 'RelayCommand' struct";

        private const string Category = "CodeStyle";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        private static readonly string GenericsMessageFormat = 
            "Do not use generic factory method for 'RelayCommand' struct";

        private static readonly DiagnosticDescriptor GenericContraintRule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, GenericContraintRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(ObjectCreationHandler, 
                SyntaxKind.ObjectCreationExpression);
            // Регистрируем новый обработчик
            context.RegisterSyntaxNodeAction(GenericNameHandler, SyntaxKind.GenericName);
        }

        private void GenericNameHandler(SyntaxNodeAnalysisContext context)
        {
            var genericNameSyntax = (GenericNameSyntax)context.Node;

            if (IsRelayCommandWasUsed(context, genericNameSyntax) && 
                (IsMethodGenericWithNewConstraint(context, genericNameSyntax) ||
                    IsActivatorCreateInstanceWasUsed(context, genericNameSyntax)))
            {
                context.ReportDiagnostic(Diagnostic.Create(GenericContraintRule, context.Node.GetLocation()));
            }
        }

        private bool IsActivatorCreateInstanceWasUsed(SyntaxNodeAnalysisContext context, GenericNameSyntax node)
        {
            var genericMethodSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
            var activatorType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Activator).FullName);

            return genericMethodSymbol?.ContainingType.Equals(activatorType) == true &&
                genericMethodSymbol?.IsGenericMethod == true;
        }

        private bool IsRelayCommandWasUsed(SyntaxNodeAnalysisContext context, GenericNameSyntax node)
        {
            var relayCommandSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(
                typeof(RelayCommand).FullName);

            return node?.TypeArgumentList.Arguments
                .OfType<IdentifierNameSyntax>()
                .Select(identifier => context.SemanticModel.GetSymbolInfo(identifier))
                .Any(symbol => symbol.Symbol?.Equals(relayCommandSymbol) == true) == true;
        }

        private bool IsMethodGenericWithNewConstraint(SyntaxNodeAnalysisContext context, GenericNameSyntax node)
        {
            var genericMethodSymbol = context.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            return genericMethodSymbol?.IsGenericMethod == true &&
                    genericMethodSymbol?.TypeParameters.Any(t => t.HasConstructorConstraint) == true;
        }

        


        private void ObjectCreationHandler(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            if (IsRelayCommandType(context, objectCreation.Type) && 
                objectCreation.ArgumentList.Arguments.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

        private bool IsRelayCommandType(SyntaxNodeAnalysisContext context, TypeSyntax type)
        {
            // Naive approach first!
            // Старый подход
            // return type.GetText().ToString().Contains(expectedType.Name) == true;

            // Получаем семантическую информацию типа
            var symbolInfo = context.SemanticModel.GetSymbolInfo(type);
            // Получаем семантическую информацию класса RelayCommand
            var relayCommandSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(
                typeof(RelayCommand).FullName);

            return symbolInfo.Symbol?.Equals(relayCommandSymbol) == true;
        }
    }
}
