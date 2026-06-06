using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Helpers.CSharp;

/// <summary>
/// Helpers for working with Roslyn statement lists.
/// </summary>
public static class StatementSyntaxExtensions
{
    /// <summary>
    /// Converts a list of statements to a normalized C# snippet.
    /// </summary>
    public static string ToSnippet(this IEnumerable<StatementSyntax> statements)
    {
        var normalizedStatements = statements
            .Select(statement => statement.NormalizeWhitespace(eol: Environment.NewLine).ToFullString())
            .ToList();

        return normalizedStatements.Count == 0
            ? string.Empty
            : string.Join(System.Environment.NewLine, normalizedStatements) + System.Environment.NewLine;
    }
}
