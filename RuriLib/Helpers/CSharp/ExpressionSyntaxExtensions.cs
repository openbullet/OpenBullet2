using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Helpers.CSharp;

/// <summary>
/// Fluent helpers for Roslyn expressions.
/// </summary>
public static class ExpressionSyntaxExtensions
{
    /// <summary>
    /// Accesses a member.
    /// </summary>
    public static MemberAccessExpressionSyntax Member(this ExpressionSyntax receiver, string memberName)
        => SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            receiver,
            SyntaxFactory.IdentifierName(memberName));

    /// <summary>
    /// Invokes an expression.
    /// </summary>
    public static InvocationExpressionSyntax Call(this ExpressionSyntax expression, params ArgumentSyntax[] arguments)
        => BlockSyntaxFactory.CreateInvocation(expression, arguments);

    /// <summary>
    /// Invokes an expression.
    /// </summary>
    public static InvocationExpressionSyntax Call(this ExpressionSyntax expression, params ExpressionSyntax[] arguments)
        => expression.Call(arguments.Select(SyntaxDsl.Arg).ToArray());

    /// <summary>
    /// Invokes an expression with the provided arguments.
    /// </summary>
    public static InvocationExpressionSyntax Call(this ExpressionSyntax expression, IEnumerable<ExpressionSyntax> arguments)
        => expression.Call(arguments.Select(SyntaxDsl.Arg).ToArray());

    /// <summary>
    /// Invokes a member.
    /// </summary>
    public static InvocationExpressionSyntax Call(this ExpressionSyntax receiver, string methodName,
        params ArgumentSyntax[] arguments)
        => receiver.Member(methodName).Call(arguments);

    /// <summary>
    /// Invokes a member.
    /// </summary>
    public static InvocationExpressionSyntax Call(this ExpressionSyntax receiver, string methodName,
        params ExpressionSyntax[] arguments)
        => receiver.Member(methodName).Call(arguments);

    /// <summary>
    /// Converts an expression to an argument.
    /// </summary>
    public static ArgumentSyntax Arg(this ExpressionSyntax expression)
        => SyntaxDsl.Arg(expression);

    /// <summary>
    /// Converts an expression to a statement.
    /// </summary>
    public static ExpressionStatementSyntax Stmt(this ExpressionSyntax expression)
        => SyntaxFactory.ExpressionStatement(expression);

    /// <summary>
    /// Creates an assignment statement.
    /// </summary>
    public static ExpressionStatementSyntax Assign(this ExpressionSyntax leftExpression, ExpressionSyntax rightExpression)
        => BlockSyntaxFactory.CreateAssignment(leftExpression, rightExpression);

    /// <summary>
    /// Awaits an expression.
    /// </summary>
    public static AwaitExpressionSyntax Await(this ExpressionSyntax expression)
        => SyntaxFactory.AwaitExpression(expression);

    /// <summary>
    /// Awaits an expression and appends <c>ConfigureAwait(false)</c>.
    /// </summary>
    public static AwaitExpressionSyntax AwaitNoCapture(this ExpressionSyntax expression)
        => BlockSyntaxFactory.CreateAwaitConfigureAwaitFalse(expression);

    /// <summary>
    /// Creates a logical AND expression.
    /// </summary>
    public static BinaryExpressionSyntax And(this ExpressionSyntax left, ExpressionSyntax right)
        => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, right);

    /// <summary>
    /// Creates a logical OR expression.
    /// </summary>
    public static BinaryExpressionSyntax Or(this ExpressionSyntax left, ExpressionSyntax right)
        => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, left, right);

    /// <summary>
    /// Creates an if statement.
    /// </summary>
    public static IfStatementSyntax If(this ExpressionSyntax condition, StatementSyntax thenStatement)
        => SyntaxFactory.IfStatement(condition, thenStatement);

    /// <summary>
    /// Creates an if/else statement.
    /// </summary>
    public static IfStatementSyntax If(this ExpressionSyntax condition, StatementSyntax thenStatement,
        StatementSyntax elseStatement)
        => SyntaxFactory.IfStatement(condition, thenStatement)
            .WithElse(SyntaxFactory.ElseClause(elseStatement));

    /// <summary>
    /// Adds an object initializer.
    /// </summary>
    public static ObjectCreationExpressionSyntax InitObject(this ObjectCreationExpressionSyntax creation,
        params ExpressionSyntax[] assignments)
    {
        if (creation.ArgumentList?.Arguments.Count == 0)
        {
            creation = creation.WithArgumentList(null);
        }

        return creation.WithInitializer(SyntaxFactory.InitializerExpression(
            SyntaxKind.ObjectInitializerExpression,
            SyntaxFactory.SeparatedList(assignments)));
    }

    /// <summary>
    /// Adds a collection initializer.
    /// </summary>
    public static ObjectCreationExpressionSyntax InitCollection(this ObjectCreationExpressionSyntax creation,
        params ExpressionSyntax[] values)
    {
        if (creation.ArgumentList?.Arguments.Count == 0)
        {
            creation = creation.WithArgumentList(null);
        }

        return creation.WithInitializer(SyntaxFactory.InitializerExpression(
            SyntaxKind.CollectionInitializerExpression,
            SyntaxFactory.SeparatedList(values)));
    }
}
