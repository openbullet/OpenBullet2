using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Helpers.CSharp;

/// <summary>
/// Parses loose C# statements by wrapping them in a synthetic method body.
/// </summary>
public static class StatementSyntaxParser
{
    /// <summary>
    /// Parses a snippet containing one or more statements.
    /// </summary>
    public static IReadOnlyList<StatementSyntax> ParseStatements(string code)
    {
        var compilationUnit = CSharpSyntaxTree.ParseText(
            $$"""
            class __CodexBlockSyntaxHost
            {
                async System.Threading.Tasks.Task __Emit()
                {
            {{code}}
                }
            }
            """).GetCompilationUnitRoot();

        return compilationUnit
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single()
            .Body?
            .Statements
            .ToList()
            ?? [];
    }
}
