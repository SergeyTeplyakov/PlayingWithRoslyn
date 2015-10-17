using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace MvvmUltraLight.Analyzer
{
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNonDefaultCtorCodeFixProvider)), Shared]
public class UseNonDefaultCtorCodeFixProvider : CodeFixProvider
{
    private const string title = "Use non-default constructor";

    public sealed override ImmutableArray<string> FixableDiagnosticIds
    {
        get { return ImmutableArray.Create(DoNotUseDefaultCtorAnalyzer.DiagnosticId); }
    }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
{
    var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

    var diagnostic = context.Diagnostics.First();
    var diagnosticSpan = diagnostic.Location.SourceSpan;

    // Фикс доступен только для вызова конструктора. Для фабричных методов ничего не получится!
    var construction = 
        root.FindToken(diagnosticSpan.Start).Parent
        .AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>()
        .FirstOrDefault();

    if (construction != null)
    {
        // Регистрируем действие, которое выполнит нужное преобразование
        var action = CodeAction.Create(
            title: title,
            createChangedDocument: c => AddEmptyLambda(context.Document, construction, c),
            equivalenceKey: title);

        context.RegisterCodeFix(action, diagnostic);
    }
}

private async Task<Document> AddEmptyLambda(
    Document document, ObjectCreationExpressionSyntax expression, CancellationToken ct)
{
    var arguments = SyntaxFactory.ParseArgumentList("(() => {})");
    var updatedNewExpression = expression.WithArgumentList(arguments);

    var root = await document.GetSyntaxRootAsync(ct);

    return document.WithSyntaxRoot(root.ReplaceNode(expression, updatedNewExpression));
}
    }
}