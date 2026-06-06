using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Helpers.CSharp;

/// <summary>
/// Short helpers for building Roslyn syntax with less boilerplate.
/// </summary>
public static class SyntaxDsl
{
    /// <summary>
    /// Gets an identifier expression.
    /// </summary>
    public static IdentifierNameSyntax Id(string name)
        => SyntaxFactory.IdentifierName(name);

    /// <summary>
    /// Parses an expression.
    /// </summary>
    public static ExpressionSyntax Expr(string expression)
        => SyntaxFactory.ParseExpression(expression);

    /// <summary>
    /// Parses a type name.
    /// </summary>
    public static TypeSyntax Type(string typeName)
        => SyntaxFactory.ParseTypeName(typeName);

    /// <summary>
    /// Creates a <c>nameof(...)</c> expression.
    /// </summary>
    public static ExpressionSyntax NameOf(string variableName)
        => Expr($"nameof({variableName})");

    /// <summary>
    /// Creates a string literal or null literal.
    /// </summary>
    public static ExpressionSyntax Lit(string? value)
        => value is null
            ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
            : SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(value));

    /// <summary>
    /// Creates a boolean literal.
    /// </summary>
    public static ExpressionSyntax Lit(bool value)
        => SyntaxFactory.LiteralExpression(
            value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

    /// <summary>
    /// Creates an integer literal.
    /// </summary>
    public static ExpressionSyntax Lit(int value)
        => Expr(CSharpWriter.ToPrimitive(value));

    /// <summary>
    /// Creates a float literal.
    /// </summary>
    public static ExpressionSyntax Lit(float value)
        => Expr(CSharpWriter.ToPrimitive(value));

    /// <summary>
    /// Creates an argument from an expression.
    /// </summary>
    public static ArgumentSyntax Arg(ExpressionSyntax expression)
        => SyntaxFactory.Argument(expression);

    /// <summary>
    /// Creates an argument from parsed text.
    /// </summary>
    public static ArgumentSyntax Arg(string expression)
        => Arg(Expr(expression));

    /// <summary>
    /// Creates an object construction expression.
    /// </summary>
    public static ObjectCreationExpressionSyntax New(string typeName, params ExpressionSyntax[] arguments)
        => SyntaxFactory.ObjectCreationExpression(Type(typeName))
            .WithArgumentList(SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(arguments.Select(Arg))));

    /// <summary>
    /// Creates an array construction expression with inferred contents.
    /// </summary>
    public static ExpressionSyntax ArrayOf(string elementType, params ExpressionSyntax[] elements)
        => SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(
                    Type(elementType),
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.ArrayRankSpecifier(
                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                SyntaxFactory.OmittedArraySizeExpression())))))
            .WithInitializer(SyntaxFactory.InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                SyntaxFactory.SeparatedList(elements)));

    /// <summary>
    /// Creates an object initializer assignment.
    /// </summary>
    public static AssignmentExpressionSyntax Prop(string name, ExpressionSyntax value)
        => SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            Id(name),
            value);

    /// <summary>
    /// Creates an object initializer assignment.
    /// </summary>
    public static AssignmentExpressionSyntax Prop(string name, string expression)
        => Prop(name, Expr(expression));

    /// <summary>
    /// Creates an object initializer from property/value pairs.
    /// </summary>
    public static IEnumerable<ExpressionSyntax> Props(params (string Name, ExpressionSyntax Value)[] values)
        => values.Select(v => (ExpressionSyntax)Prop(v.Name, v.Value));
}
