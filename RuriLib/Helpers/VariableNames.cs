using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace RuriLib.Helpers
{
    /// <summary>
    /// Utility class that generates C# variable names.
    /// </summary>
    public static class VariableNames
    {
        /// <summary>
        /// Checks if a <paramref name="name"/> is a valid C# variable name.
        /// </summary>
        public static bool IsValid(string name)
        {
            if (name.Contains("."))
                return name.Split('.').All(IsValid);

            var provider = CodeDomProvider.CreateProvider("C#");
            return provider.IsValidIdentifier(name);
        }

        /// <summary>
        /// Turns a possibly invalid <paramref name="name"/> into a valid C# variable name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string MakeValid(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (IsValid(name))
                return name;

            var replaced = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

            if (replaced == string.Empty)
                return RandomName();

            if (!char.IsLetter(replaced[0]) && replaced[0] != '_')
                replaced = '_' + replaced;

            return replaced;
        }

        /// <summary>
        /// Gets a random valid C# alphabetic variable name with a given <paramref name="length"/>.
        /// </summary>
        public static string RandomName(int length = 4)
        {
            var rand = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
