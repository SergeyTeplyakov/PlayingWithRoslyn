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
    public class DoNotUseDefaultContructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DoNotUseStructDefaultContructor";

        private const string NoDefaultContructorAttributeName = "NoDefaultConstructorAttribute";
         
        private static readonly string DefaultConstructorTitle = "Do not struct default constructor";
        private static readonly string FactoryMethodTitle = "Do not use factory methods for struct that calls default ctor";
        private static readonly string MessageFormat = $"Do not use default constructor for struct '{{0}}' that marked with '{NoDefaultContructorAttributeName}'";
        private static readonly string FactoryMethodMessageFormat = $"Do not use factory methods for struct '{{0}}' that marked with '{NoDefaultContructorAttributeName}'";
        private const string Category = "CodeStyle";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, DefaultConstructorTitle, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor GenericContraintRule = new DiagnosticDescriptor(DiagnosticId, FactoryMethodTitle, FactoryMethodMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, GenericContraintRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(ObjectCreationHandler, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(GenericNameHandler, SyntaxKind.GenericName);
        }

        private bool MarkedWithNoDefaultContstructorAttribute(ISymbol symbol)
        {
            return symbol?.GetAttributes()
                .Any(a => a.AttributeClass.Name.Equals(
                    NoDefaultContructorAttributeName, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private void GenericNameHandler(SyntaxNodeAnalysisContext context)
        {
            var genericNameSyntax = (GenericNameSyntax)context.Node;
            
            var markedWithNoDefaultCtor = genericNameSyntax?.TypeArgumentList.Arguments
                .OfType<IdentifierNameSyntax>()
                .Select(identifier => context.SemanticModel.GetSymbolInfo(identifier))
                .Any(symbol => MarkedWithNoDefaultContstructorAttribute(symbol.Symbol));

            if (markedWithNoDefaultCtor == true)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(genericNameSyntax).Symbol;

                if ((IsActivator(context, symbol) && IsGeneric(symbol)) ||
                    IsGenericMethodWithNewConstraint(symbol) ||
                    IsGenericTypeWithNewConstraint(symbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(GenericContraintRule, context.Node.GetLocation()));
                }
            }
        }

        private static bool IsGeneric(ISymbol symbol)
        {
            return
                (symbol as IMethodSymbol)?.IsGenericMethod == true ||
                (symbol as INamedTypeSymbol)?.IsGenericType == true;
        }

        private static bool IsActivator(SyntaxNodeAnalysisContext context, ISymbol symbol)
        {
            var activatorType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Activator).FullName);

            // Activator.CreateInstance<T> is a special case, because this method does not have new() constraint!
            return (symbol as IMethodSymbol)?.ContainingType.Equals(activatorType) == true;
        }

        private static bool IsGenericMethodWithNewConstraint(ISymbol symbol)
        {
            var genericMethodSymbol = symbol as IMethodSymbol;
            return genericMethodSymbol?.TypeParameters.Any(tp => tp.HasConstructorConstraint) == true;
        }

        private static bool IsGenericTypeWithNewConstraint(ISymbol symbol)
        {
            var genericMethodSymbol = symbol as INamedTypeSymbol;
            return genericMethodSymbol?.TypeParameters.Any(tp => tp.HasConstructorConstraint) == true;
        }

        private void ObjectCreationHandler(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax) context.Node;
            var typeInfo = context.SemanticModel.GetSymbolInfo(objectCreation.Type);
            if (MarkedWithNoDefaultContstructorAttribute(typeInfo.Symbol) && objectCreation.ArgumentList.Arguments.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}
