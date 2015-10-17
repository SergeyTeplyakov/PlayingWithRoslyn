using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpConsoleCodeHighlighter
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = SyntaxFactory.ParseArgumentList("(() => {})");
            Console.WriteLine(arguments);
            SyntaxTree tree = CSharpSyntaxTree.ParseText("var x = new Foo(() => {})");
            var root = tree.GetRoot();
            var d = SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)tree.GetRoot());
            Console.WriteLine(tree);
            //// Printing content for the current file!
            //var currentFile = @"..\..\Program.cs";
            //ConsoleHighlighter.PrintSourceFromFileAsync(currentFile).Wait();

            //#region Printing to Console
            //const int answer = 42;
            //Console.WriteLine("Done! " + answer + " ;)");
            //#endregion

            Console.ReadLine();
        }
    }
}
