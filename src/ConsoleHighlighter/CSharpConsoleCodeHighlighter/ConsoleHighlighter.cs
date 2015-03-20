using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpConsoleCodeHighlighter
{
    public static class ConsoleHighlighter
    {
        public static async Task PrintSourceAsync(string sourceCode)
        {
            // Opening a file to newly created workspace
            Document document = CreateDocumentFrom(sourceCode);
            var syntaxTreeRoot = await document.GetSyntaxRootAsync();

            Console.BackgroundColor = ConsoleColor.Black;

            // Getting classification for the document
            IEnumerable<ClassifiedSpan> classification =
                await Classifier.GetClassifiedSpansAsync(document, syntaxTreeRoot.FullSpan);

            // Getting a map from position to a new color
            IDictionary<int, ConsoleColor> positionColorMap =
                classification.ToDictionary(c => c.TextSpan.Start, c => GetColorFor(c.ClassificationType));

            // Iterating over each character in source file and printing it
            for (int charPosition = 0; charPosition < sourceCode.Length; charPosition++)
            {
                // Check whether new color should be used starting from current position
                ConsoleColor newColor;
                if (positionColorMap.TryGetValue(charPosition, out newColor))
                {
                    Console.ForegroundColor = newColor;
                }

                Console.Write(sourceCode[charPosition]);
            }

            Console.ResetColor();
        }

        private static Document CreateDocumentFrom(string sourceCode)
        {
            var workspace = new AdhocWorkspace();
            Solution solution = workspace.CurrentSolution;
            Project project = solution.AddProject("SyntaxHighlighter", "SyntaxHighlighter", LanguageNames.CSharp);
            Document document = project.AddDocument("source.cs", sourceCode);
            return document;
        }

        public static async Task PrintSourceFromFileAsync(string fileName)
        {
            using (var file = File.OpenText(fileName))
            {
                var content = await file.ReadToEndAsync();
                await PrintSourceAsync(content);
            }
        }

        private static ConsoleColor GetColorFor(string classificatioName)
        {
            switch (classificatioName)
            {
                case ClassificationTypeNames.InterfaceName:
                case ClassificationTypeNames.EnumName:
                case ClassificationTypeNames.Keyword:
                    return ConsoleColor.DarkCyan;

                case ClassificationTypeNames.ClassName:
                case ClassificationTypeNames.StructName:
                    return ConsoleColor.DarkYellow;

                case ClassificationTypeNames.Identifier:
                    return ConsoleColor.DarkGray;

                case ClassificationTypeNames.Comment:
                    return ConsoleColor.DarkGreen;

                case ClassificationTypeNames.StringLiteral:
                case ClassificationTypeNames.VerbatimStringLiteral:
                    return ConsoleColor.DarkRed;

                case ClassificationTypeNames.Punctuation:
                    return ConsoleColor.Gray;

                case ClassificationTypeNames.WhiteSpace:
                    return ConsoleColor.Black;

                case ClassificationTypeNames.NumericLiteral:
                    return ConsoleColor.DarkYellow;

                case ClassificationTypeNames.PreprocessorKeyword:
                    return ConsoleColor.DarkMagenta;
                case ClassificationTypeNames.PreprocessorText:
                    return ConsoleColor.DarkGreen;

                default:
                    return ConsoleColor.Gray;
            }
        }
    }
}
