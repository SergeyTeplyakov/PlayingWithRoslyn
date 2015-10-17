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
    public class StructWithNoDefaultContructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "StructWithNoDefaultContructorAnalyzer";

        private const string NoDefaultContructorAttributeName = "NoDefaultConstructorAttribute";
        private const string NoDefaultContructorAttributeShortName = "NoDefaultConstructor";
         
        private static readonly string Title = $"No default constructor on struct";
        private static readonly string MessageFormat = $"Struct '{{0}}' that marked with {NoDefaultContructorAttributeName} should have non-default constructor";
        private const string Category = "CodeStyle";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(StructDeclarationHandler, SyntaxKind.StructDeclaration);
        }

        private void StructDeclarationHandler(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            
            if (MarkedWithNoDefaultContstructorAttribute(structDeclaration))
            {
                if (!HasNonDefaultConstructor(structDeclaration))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), structDeclaration.Identifier.Text));
                }
                // More rules could be added. Like when struct does have non-default constructor but non of them is accessible.
            }
        }

        private bool HasNonDefaultConstructor(StructDeclarationSyntax syntax)
        {
            return syntax.Members.OfType<ConstructorDeclarationSyntax>().Any();
        }

        private bool MarkedWithNoDefaultContstructorAttribute(StructDeclarationSyntax syntax)
        {
            return syntax.AttributeLists.SelectMany(a => a.Attributes)
                .Any(a => 
                        a.Name.GetText().ToString() == NoDefaultContructorAttributeName 
                        || a.Name.GetText().ToString() == NoDefaultContructorAttributeShortName);
        }
    }
}
