using Newtonsoft.Json;
using OpenQA.Selenium;
using RuriLib.Extensions;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Captchas;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    public abstract class BlockBase
    {
        public string Label { get; set; }
        public bool Disabled { get; set; }

        /// <summary>Whether the block is a selenium-related block.</summary>
        [JsonIgnore]
        public bool IsSelenium => GetType().ToString().StartsWith("S");

        /// <summary>Whether the block is a captcha-related block.</summary>
        [JsonIgnore]
        public bool IsCaptcha => GetType().Name.Contains("Captcha", StringComparison.OrdinalIgnoreCase);

        #region Virtual methods
        /// <summary>
        /// Builds a block from a line of LoliScript code.
        /// </summary>
        public virtual BlockBase FromLS(string line) => throw new Exception("Cannot Convert to the abstract class BlockBase");

        /// <summary>
        /// Builds a block from multiple lines of LoliScript code.
        /// </summary>
        public virtual BlockBase FromLS(List<string> lines) => throw new Exception("Cannot Convert from the abstract class BlockBase");

        /// <summary>
        /// Converts the block to LoliScript code.
        /// </summary>
        public virtual string ToLS(bool indent = true) => throw new Exception("Cannot Convert from the abstract class BlockBase");

        /// <summary>
        /// Executes the actual block logic.
        /// </summary>
        public virtual Task Process(LSGlobals ls)
        {
            ls.BotData.Logger.Log($">> Executing Block {Label} <<", LogColors.ChromeYellow);
            return Task.CompletedTask;
        }
        #endregion

        #region Variable Replacement
        /// <summary>
        /// Replaces variables recursively, expanding lists or dictionaries with jolly indices.
        /// </summary>
        public static List<string> ReplaceValuesRecursive(string original, LSGlobals ls)
        {
            var data = ls.BotData;
            var globals = ls.Globals;
            var toReplace = new List<string>();

            // Regex parse the syntax <LIST[*]>
            var matches = Regex.Matches(original, @"<([^\[]*)\[\*\]>");
            var variables = new List<ListOfStringsVariable>();

            foreach (Match m in matches)
            {
                var name = m.Groups[1].Value;

                // Retrieve the variable
                var variable = GetVariables(data).Get(name);

                // If it's null, try to get it from the global variables
                if (variable == null)
                {
                    variable = globals.Get(name);
                    
                    // If still null, there's nothing to replace, skip it
                    if (variable == null)
                    {
                        continue;
                    }
                }

                // Make sure it's a List of strings and add it to the list
                if (variable is ListOfStringsVariable list)
                {
                    variables.Add(list);
                }
            }

            // If there's no corresponding variable, just readd the input string and proceed with normal replacement
            if (variables.Count > 0)
            {
                // Example: we have 3 lists of sizes 3, 7 and 5. We need to take 7
                var max = variables.Max(v => v.AsListOfStrings().Count);

                for (var i = 0; i < max; i++)
                {
                    var replaced = original;

                    foreach (var variable in variables)
                    {
                        var list = variable.AsListOfStrings();
                        replaced = list.Count > i ? replaced.Replace($"<{variable.Name}[*]>", list[i]) : replaced.Replace($"<{variable.Name}[*]>", "NULL");
                    }

                    toReplace.Add(replaced);
                }
                goto END;
            }

            // Regex parse the syntax <DICT(*)> (wildcard key -> returns list of all values)
            var match = Regex.Match(original, @"<([^\(]*)\(\*\)>");

            if (match.Success)
            {
                var full = match.Groups[0].Value;
                var name = match.Groups[1].Value;

                // Retrieve the dictionary
                var dict = GetVariables(data).Get<DictionaryOfStringsVariable>(name) ?? globals.Get<DictionaryOfStringsVariable>(name);

                // If there's no corresponding variable, just readd the input string and proceed with normal replacement
                if (dict == null)
                {
                    toReplace.Add(original);
                }
                else
                {
                    foreach (var item in dict.AsDictionaryOfStrings())
                    {
                        toReplace.Add(original.Replace(full, item.Value));
                    }
                }
                goto END;
            }

            // Regex parse the syntax <DICT{*}> (wildcard value -> returns list of all keys)
            match = Regex.Match(original, @"<([^\{]*)\{\*\}>");

            if (match.Success)
            {
                var full = match.Groups[0].Value;
                var name = match.Groups[1].Value;

                // Retrieve the dictionary
                var dict = GetVariables(data).Get<DictionaryOfStringsVariable>(name);

                if (dict == null)
                {
                    if (name == "COOKIES")
                    {
                        dict = new DictionaryOfStringsVariable(data.COOKIES) { Name = name };
                    }
                    else if (name == "HEADERS")
                    {
                        dict = new DictionaryOfStringsVariable(data.HEADERS) { Name = name };
                    }
                    else
                    {
                        dict = globals.Get<DictionaryOfStringsVariable>(name);
                    }
                }

                // If there's no corresponding variable, just readd the input string and proceed with normal replacement
                if (dict == null)
                {
                    toReplace.Add(original);
                }
                else
                {
                    foreach (var item in dict.AsDictionaryOfStrings())
                    {
                        toReplace.Add(original.Replace(full, item.Key));
                    }
                }
                goto END;
            }

            // If no other match was a success, it means there's no recursive value and we simply add the input to the list
            toReplace.Add(original);

            END:
            // Now for each item in the list, do the normal replacement and return the replaced list of strings
            return toReplace.Select(i => (string)ReplaceValues(i, ls)).ToList();
        }

        /// <summary>
        /// Replaces variables in all keys and values of a dictionary.
        /// </summary>
        public static Dictionary<string, string> ReplaceValues(Dictionary<string, string> original, LSGlobals ls)
        {
            var newDict = new Dictionary<string, string>();

            foreach (var kvp in original)
            {
                newDict[ReplaceValues(kvp.Key, ls)] = ReplaceValues(kvp.Value, ls);
            }

            return newDict;
        }

        /// <summary>
        /// Replaces variables in all items of a list.
        /// </summary>
        public static List<string> ReplaceValues(List<string> original, LSGlobals ls)
            => original.Select(i => ReplaceValues(i, ls)).ToList();

        /// <summary>
        /// Replaces variables in a given input string.
        /// </summary>
        public static string ReplaceValues(string original, LSGlobals ls)
        {
            if (original == null)
            {
                return string.Empty;
            }

            var data = ls.BotData;
            var globals = ls.Globals;

            if (!original.Contains("<") && !original.Contains(">"))
            {
                return original;
            }

            var previous = "";
            var output = original;

            do
            {
                previous = output;

                // Replace all the fixed quantities
                output = output.Replace("<INPUT>", data.Line.Data);
                output = output.Replace("<STATUS>", data.STATUS);
                output = output.Replace("<SOURCE>", data.SOURCE);
                output = output.Replace("<COOKIES>", data.COOKIES.AsString());
                output = output.Replace("<HEADERS>", data.HEADERS.AsString());
                output = output.Replace("<RESPONSECODE>", data.RESPONSECODE.AsString());
                output = output.Replace("<ADDRESS>", data.ADDRESS);
                output = output.Replace("<RETRIES>", data.Line.Retries.ToString());

                var lastCaptchaInfo = data.TryGetObject<CaptchaInfo>("lastCaptchaInfo");
                if (lastCaptchaInfo is not null)
                {
                    output = output.Replace("<CAPTCHAID>", lastCaptchaInfo.Id.ToString());
                }

                // TODO: Readd this
                // output = output.Replace("<BOTNUM>", data.BotNumber.ToString());

                if (data.Proxy != null)
                {
                    output = output.Replace("<PROXY>", data.Proxy.ToString());
                }

                // Get all the inner (max. 1 level of nesting) variables
                var matches = Regex.Matches(output, @"<([^<>]*)>");
                
                foreach (Match match in matches)
                {
                    var full = match.Groups[0].Value;
                    var m = match.Groups[1].Value;

                    // Parse the variable name
                    var name = Regex.Match(m, @"^[^\[\{\(]*").Value;

                    // Try to get the variable (first local, then global, then if none was found go to the next iteration)
                    // We don't throw an error here because it could be some HTML or XML code e.g. <br> that triggers this, and we dont' want to spam the user with unneeded errors
                    var v = GetVariables(data).Get(name);

                    if (v == null)
                    {
                        if (name == "COOKIES")
                        {
                            v = new DictionaryOfStringsVariable(data.COOKIES) { Name = name };
                        }
                        else if (name == "HEADERS")
                        {
                            v = new DictionaryOfStringsVariable(data.HEADERS) { Name = name };
                        }
                        else
                        {
                            v = globals.Get(name);
                        }
                    }

                    if (v == null)
                    {
                        continue;
                    }

                    // Parse the arguments
                    var args = m.Replace(name, "");

                    switch (v)
                    {
                        case StringVariable:
                            output = output.Replace(full, v.AsString());
                            break;

                        case ListOfStringsVariable:

                            // If it's just the list name, replace it with its string representation
                            if (string.IsNullOrEmpty(args))
                            {
                                output = output.Replace(full, v.AsString());
                                break;
                            }

                            int.TryParse(ParseArguments(args, '[', ']')[0], out var index);
                            var item = GetListItem(v.AsListOfStrings(), index); // Can return null

                            if (item != null)
                            {
                                output = output.Replace(full, item);
                            }
                            break;

                        case DictionaryOfStringsVariable:

                            var dict = v.AsDictionaryOfStrings();

                            if (args.Contains("(") && args.Contains(")"))
                            {
                                var key = ParseArguments(args, '(', ')')[0];
                                
                                if (dict.ContainsKey(key))
                                {
                                    output = output.Replace(full, dict[key]);
                                }
                            }
                            else if (args.Contains("{") && args.Contains("}"))
                            {
                                var value = ParseArguments(args, '{', '}')[0];
                                
                                if (dict.ContainsValue(value))
                                {
                                    output = output.Replace(full, dict.First(kvp => kvp.Value == value).Key);
                                }
                            }
                            else // If it's just the dictionary name, replace it with its string representation
                            {
                                output = output.Replace(full, v.AsString());
                                break;
                            }
                            break;
                    }
                }
            }
            while (original.Contains("<") && original.Contains(">") && output != previous);

            return output;
        }
        #endregion

        /// <summary>
        /// Parses an argument between two bracket delimiters.
        /// </summary>
        /// <param name="input">The string to parse the argument from</param>
        /// <param name="delimL">The left bracket delimiter</param>
        /// <param name="delimR">The right bracket delimiter</param>
        /// <returns>The argument between the delimiters</returns>
        protected static List<string> ParseArguments(string input, char delimL, char delimR)
        {
            var output = new List<string>();
            var matches = Regex.Matches(input, @"\" + delimL + @"([^\" + delimR + @"]*)\" + delimR);
            foreach (Match match in matches) output.Add(match.Groups[1].Value);
            return output;
        }

        /// <summary>
        /// Updates the ADDRESS and SOURCE variables basing on the selenium-driven browser's URL bar and page source.
        /// </summary>
        /// <param name="data">The BotData containing the driver and the variables</param>
        protected static void UpdateSeleniumData(BotData data)
        {
            var browser = data.TryGetObject<WebDriver>("selenium");

            if (browser != null)
            {
                data.ADDRESS = browser.Url;
                data.SOURCE = browser.PageSource;
            }
        }

        #region Variable Insertion
        /// <summary>
        /// Adds a single variable with the given value.
        /// </summary>
        protected static void InsertVariable(LSGlobals ls, bool isCapture, string value, string variableName,
            string prefix = "", string suffix = "", bool urlEncode = false, bool createEmpty = true)
            => InsertVariable(ls, isCapture, false, new string[] { value }, variableName, prefix, suffix, urlEncode, createEmpty);

        /// <summary>
        /// Adds a list variable with the given value.
        /// </summary>
        protected static void InsertVariable(LSGlobals ls, bool isCapture, IEnumerable<string> values, string variableName,
            string prefix = "", string suffix = "", bool urlEncode = false, bool createEmpty = true)
            => InsertVariable(ls, isCapture, true, values, variableName, prefix, suffix, urlEncode, createEmpty);

        /// <summary>
        /// Adds a single or list variable with the given value.
        /// </summary>
        protected static void InsertVariable(LSGlobals ls, bool isCapture, bool recursive, IEnumerable<string> values, string variableName,
            string prefix = "", string suffix = "", bool urlEncode = false, bool createEmpty = true)
        {
            var data = ls.BotData;
            var list = values.Select(v => ReplaceValues(prefix, ls) + v.Trim() + ReplaceValues(suffix, ls)).ToList();
            
            if (urlEncode)
            {
                list = list.Select(Uri.EscapeDataString).ToList();
            }

            Variable variable = null;
            
            if (recursive)
            {
                if (list.Count > 0 || createEmpty)
                {
                    variable = new ListOfStringsVariable(list)
                    {
                        Name = variableName
                    };
                }
            }
            else
            {
                if (list.Count == 0)
                {
                    if (createEmpty)
                    {
                        variable = new StringVariable(string.Empty)
                        {
                            Name = variableName
                        };
                    }
                }
                else
                {
                    variable = new StringVariable(list.First())
                    {
                        Name = variableName
                    };
                }
            }

            if (variable != null)
            {
                GetVariables(data).Set(variable);
                data.Logger.Log($"Parsed variable | Name: {variable.Name} | Value: {variable.AsString()}", isCapture ? LogColors.OrangeRed : LogColors.Gold);
                variable.MarkedForCapture = isCapture;
            }
            else
            {
                data.Logger.Log("Could not parse any data. The variable was not created.", LogColors.White);
            }
        }

        public static VariablesList GetVariables(BotData data) => data.TryGetObject<VariablesList>("legacyVariables");
        #endregion

        private static string GetListItem(List<string> list, int index)
        {
            // If the index is negative, start from the end
            if (index < 0)
            {
                // For example in a [1,2,3] list, the element at -1 is at index 3-1 = 2 which is element '3'
                index = list.Count + index;
            }

            return index > list.Count - 1 || index < 0 ? null : list[index];
        }
    }
}
