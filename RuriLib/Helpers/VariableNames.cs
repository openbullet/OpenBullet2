using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace RuriLib.Helpers;

/// <summary>
/// Utility class that generates C# variable names.
/// </summary>
public static class VariableNames
{
    private static readonly CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");

    /// <summary>
    /// Checks if a <paramref name="name"/> is a valid C# variable name.
    /// </summary>
    public static bool IsValid(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.Contains('.'))
        {
            return name.Split('.').All(IsValid);
        }

        return provider.IsValidIdentifier(name);
    }

    /// <summary>
    /// Turns a possibly invalid <paramref name="name"/> into a valid C# variable name.
    /// </summary>
    /// <param name="name">The original variable name.</param>
    /// <returns>A valid C# variable name.</returns>
    public static string MakeValid(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (IsValid(name))
        {
            return name;
        }

        if (name.Contains('.'))
        {
            return string.Join('.', name.Split('.').Select(MakeValid));
        }

        var replaced = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

        if (replaced == string.Empty)
        {
            return RandomName();
        }

        if (!char.IsLetter(replaced[0]) && replaced[0] != '_')
        {
            replaced = '_' + replaced;
        }

        return replaced;
    }

    /// <summary>
    /// Gets a random valid C# alphabetic variable name with a given <paramref name="length"/>.
    /// </summary>
    public static string RandomName(int length = 4)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";

        return string.Create(length, chars, static (buffer, alphabet) =>
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = alphabet[Random.Shared.Next(alphabet.Length)];
            }
        });
    }
}
