using System;
using System.Text;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Pretty prints an exception and all its inner exceptions.
    /// </summary>
    public static string PrettyPrint(this Exception ex)
    {
        var sb = new StringBuilder();

        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (sb.Length > 0)
            {
                sb.Append(" | ");
            }

            sb.Append($"{current.GetType()}: {current.Message}");
        }

        return sb.ToString();
    }
}
