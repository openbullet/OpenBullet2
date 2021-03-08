using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace RuriLib.Helpers
{
    public static class VariableNames
    {
        public static bool IsValid(string name)
        {
            if (name.Contains("."))
                return name.Split('.').All(n => IsValid(n));

            var provider = CodeDomProvider.CreateProvider("C#");
            return provider.IsValidIdentifier(name);
        }

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

        public static string RandomName(int length = 4)
        {
            var rand = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
