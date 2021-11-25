using RuriLib.Legacy.Blocks;
using RuriLib.Legacy.Models;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RuriLib.Legacy.LS
{
    /// <summary>
    /// Contains methods used to parse tokens from a LoliScript line of code.
    /// </summary>
    public static class LineParser
    {
        /// <summary>
        /// Parses a token of a given type from a line.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="type">The type of token to parse</param>
        /// <param name="essential">Whether an exception should be thrown if the token cannot be parsed</param>
        /// <param name="proceed">Whether to remove the token from the original line after parsing it</param>
        /// <returns>The parsed token or, if parse fails but the token is not essential, an empty string</returns>
        public static string ParseToken(ref string input, TokenType type, bool essential, bool proceed = true)
        {
            var pattern = GetPattern(type);
            var token = "";

            var r = new Regex(pattern);
            var m = r.Match(input);
            
            if (m.Success)
            {
                token = m.Value;

                if (proceed)
                {
                    input = input.Substring(token.Length).Trim();
                }

                if (type == TokenType.Literal)
                {
                    token = token.Substring(1, token.Length - 2).Replace("\\\\", "\\").Replace("\\\"", "\"");
                }
            }
            else
            {
                if (essential)
                {
                    throw new ArgumentException("Cannot parse token");
                }
            }

            return token;
        }

        /// <summary>
        /// Sets a boolean property by parsing its name and value from a line.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="instance">The instance of the object containing the boolean property</param>
        public static void SetBool(ref string input, object instance)
        {
            var result = ParseToken(ref input, TokenType.Parameter, true, true).Split('=');
            PropertyInfo prop;

            try
            {
                prop = instance.GetType().GetProperty(result[0]);
            }
            catch
            {
                return;
                // throw new ArgumentException($"There is no property called {result[0]} in the type {instance.GetType().ToString()}");
            }

            var propVal = prop.GetValue(instance);
            
            if (propVal.GetType() != typeof(bool))
            {
                throw new InvalidCastException($"The property {result[0]} is not a boolean");
            }

            switch (result[1].ToUpper())
            {
                case "TRUE":
                    prop.SetValue(instance, true);
                    break;

                case "FALSE":
                    prop.SetValue(instance, false);
                    break;

                default:
                    throw new ArgumentException($"Expected bool value for '{prop.Name}'");
            }
        }

        /// <summary>
        /// Parses an enum value from a line.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="label">Debug information about the expected enum</param>
        /// <param name="enumType">The type of the enum</param>
        /// <returns>An enum of the provided enumType</returns>
        public static dynamic ParseEnum(ref string input, string label, Type enumType)
        {
            // Parse the token first
            string token;
            
            try
            {
                token = ParseToken(ref input, TokenType.Parameter, true);
            }
            catch
            {
                throw new ArgumentException($"Missing '{label}'");
            }
            
            try
            {
                return Enum.Parse(enumType, token, true);
            }
            catch
            {
                throw new ArgumentException($"Invalid '{label}'");
            }
        }

        /// <summary>
        /// Parses a literal value from a line.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="label">Debug information about the expected literal</param>
        /// <param name="replace">Whether to perform variable replacement in the literal</param>
        /// <returns>The literal without the leading and trailing double quotes</returns>
        public static string ParseLiteral(ref string input, string label, bool replace = false, LSGlobals ls = null)
        {
            try
            {
                return replace
                    ? BlockBase.ReplaceValues(ParseToken(ref input, TokenType.Literal, true), ls)
                    : ParseToken(ref input, TokenType.Literal, true);
            }
            catch
            {
                throw new ArgumentException($"Expected Literal value for '{label}'");
            }
        }

        /// <summary>
        /// Parses an integer value from a line.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="label">Debug information about the expected integer</param>
        /// <returns>The integer value</returns>
        public static int ParseInt(ref string input, string label)
        {
            try
            {
                return int.Parse(ParseToken(ref input, TokenType.Parameter, true));
            }
            catch
            {
                throw new ArgumentException($"Expected Integer value for '{label}'");
            }
        }

        /// <summary>
        /// Parses a block label from a line.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <returns>The label of the block, if defined</returns>
        public static string ParseLabel(ref string input) => ParseToken(ref input, TokenType.Label, false).Substring(1);

        /// <summary>
        /// Makes sure that a specified identifier is present and moves past it. An exception will be thrown if the identifier is not present.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="id">The expected identifier</param>
        public static void EnsureIdentifier(ref string input, string id)
        {
            var token = ParseToken(ref input, TokenType.Parameter, true);

            if (!token.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Expected identifier '{id}'");
            }
        }

        /// <summary>
        /// Gets the type of the first token in the given line of code.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <returns>The type of the token</returns>
        public static TokenType Lookahead(ref string input)
        {
            var token = ParseToken(ref input, TokenType.Parameter, true, false);

            if (token.Contains("\""))
            {
                return TokenType.Literal;
            }
            else if (token == "->")
            {
                return TokenType.Arrow;
            }
            else if (token.StartsWith("#"))
            {
                return TokenType.Label;
            }
            else if (token.ToUpper().Contains("=TRUE") || token.ToUpper().Contains("=FALSE"))
            {
                return TokenType.Boolean;
            }
            else if (int.TryParse(token, out _))
            {
                return TokenType.Integer;
            }
            else
            {
                return TokenType.Parameter;
            }
        }

        /// <summary>
        /// Checks if the next token is a given identifier.
        /// </summary>
        /// <param name="input">The reference to the line of code</param>
        /// <param name="id">The identifier to check</param>
        /// <returns>Whether the token and the identifier are equal</returns>
        public static bool CheckIdentifier(ref string input, string id)
        {
            try
            {
                var token = ParseToken(ref input, TokenType.Parameter, true, false);
                return token.Equals(id, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the regex pattern to match a given token type.
        /// </summary>
        /// <param name="type">The token type</param>
        /// <returns>The regex pattern to parse the token type</returns>
        private static string GetPattern(TokenType type)
            => type switch
            {
                TokenType.Label => "^#[^ ]*",
                TokenType.Parameter => "^[^ ]*",
                TokenType.Literal => "\"(\\\\.|[^\\\"])*\"",
                TokenType.Arrow => "^->",
                _ => ""
            };
    }

    /// <summary>
    /// The allowed types of tokens that can be parsed from a line.
    /// </summary>
    public enum TokenType
    {
        /// <summary>A block label.</summary>
        Label,

        /// <summary>A generic parameter, usually an enum value.</summary>
        Parameter,

        /// <summary>A string between double quotes.</summary>
        Literal,

        /// <summary>The character sequence -&gt;.</summary>
        Arrow,

        /// <summary>A boolean value in the format Name=Value where Name is the name of the property.</summary>
        Boolean,

        /// <summary>An integer value.</summary>
        Integer
    }
}
