# Playing With Roslyn

Playing with roslyn is a set of very simple samples that I'm using to learn Roslyn.

## Console Syntax Highlighter
Console Syntax visualizer is a naive console app that prints it's own content to the concole with syntax highlighting.

Here is a small example:

```c#
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
            // Printing content for the current file!
            var currentFile = @"..\..\Program.cs";
            ConsoleHighlighter.PrintSourceFromFileAsync(currentFile).Wait();

            #region Printing to Console
            const int answer = 42;
            Console.WriteLine("Done! " + answer + " ;)");
            #endregion

            Console.ReadLine();
        }
    }
}
```
And here is an output of this tool:

![Image](https://github.com/SergeyTeplyakov/PlayingWithRoslyn/raw/master/img/RoslynHighlighter.png)