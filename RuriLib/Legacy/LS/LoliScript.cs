using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Jint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.LS
{
    /// <summary>
    /// Represents a LoliScript script that can be run line by line.
    /// </summary>
    public class LoliScript
    {
        /// <summary>The actual script as a string containing linebreaks.</summary>
        public string Script { get; set; }

        // Runtime counter
        private int i = 0;
        private string[] lines = new string[] { };

        // Needed for BEGIN SCRIPT directives
        private string otherScript = "";
        private ScriptingLanguage language = ScriptingLanguage.JavaScript;

        /// <summary>The current line being processed.</summary>
        public string CurrentLine { get; set; } = "";

        /// <summary>The next block to be processed. Empty if the script has no more blocks to execute.</summary>
        public string NextBlock
        {
            get
            {
                for (int j = i; j < lines.Count(); j++)
                {
                    var line = lines[j];
                    if (IsEmptyOrCommentOrDisabled(line) || !BlockParser.IsBlock(line)) continue;

                    var label = "";
                    if (lines[j].StartsWith("#")) label = LineParser.ParseLabel(ref line);
                    var blockName = LineParser.ParseToken(ref line, TokenType.Parameter, false, false);

                    if (label != string.Empty) return $"{blockName} ({label})";
                    else return blockName;
                }
                return "";
            }
        }

        /// <summary>The current block being processed.</summary>
        public string CurrentBlock { get; set; } = "";

        /// <summary>Whether the script can proceed the execution or not.</summary>
        public bool CanProceed
        {
            get
            {
                return i < lines.Count() && lines.Skip(i).Any(l => !IsEmptyOrCommentOrDisabled(l));
            }
        }

        /// <summary>
        /// Constructs a LoliScript object with an empty script.
        /// </summary>
        public LoliScript()
        {
            Script = "";
        }

        /// <summary>
        /// Constructs a LoliScript object with a given script.
        /// </summary>
        /// <param name="script">The LoliScript script</param>
        public LoliScript(string script)
        {
            Script = script;
        }

        #region Conversion
        /// <summary>
        /// Transforms the script into a list of blocks. The blocks that cannot be converted will be created as BlockLSCode blocks.
        /// </summary>
        /// <returns></returns>
        public List<BlockBase> ToBlocks()
        {
            List<BlockBase> list = new List<BlockBase>();
            var compressed = CompressedLines;
            List<string> buffer = new List<string>();
            var isScript = false;

            foreach (var c in compressed.Where(c => !string.IsNullOrEmpty(c.Trim())))
            {
                if (!isScript && BlockParser.IsBlock(c))
                {
                    if (buffer.Count > 0)
                    {
                        var b = new BlockLSCode();
                        list.Add(b.FromLS(buffer));
                        buffer.Clear();
                    }

                    try
                    {
                        list.Add(BlockParser.Parse(c));
                    }
                    catch (Exception ex)
                    {
                        var line = c;
                        line = BlockBase.TruncatePretty(line, 50);
                        throw new Exception($"Exception while parsing block {line}\nReason: {ex.Message}");
                    }
                }
                else
                {
                    buffer.Add(c);

                    if (c.StartsWith("BEGIN SCRIPT")) isScript = true;
                    else if (c.StartsWith("END SCRIPT")) isScript = false;
                }
            }

            if (buffer.Count > 0)
            {
                var b = new BlockLSCode();
                list.Add(b.FromLS(buffer));
                buffer.Clear();
            }

            return list;
        }

        /// <summary>
        /// Sets the script from a list of blocks.
        /// </summary>
        /// <param name="blocks">The list of blocks which inherit from the BlockBase type</param>
        public void FromBlocks(List<BlockBase> blocks)
        {
            Script = "";

            foreach (var block in blocks)
            {
                Script += block.ToLS() + Environment.NewLine + Environment.NewLine;
            }
        }
        #endregion

        /// <summary>
        /// Resets the line counter so the script can be run again.
        /// </summary>
        public void Reset()
        {
            i = 0;
            otherScript = "";
            language = ScriptingLanguage.JavaScript;

            // Separate the script into lines
            lines = Regex.Split(Script, "\r\n|\r|\n");
        }

        /// <summary>
        /// Executes a line of the script.
        /// </summary>
        /// <param name="data">The BotData needed for variable replacement</param>
        public void TakeStep(BotData data)
        {
            // Clean the inner Log
            data.LogBuffer.Clear();

            // TODO: Refactor this with a properly written policy
            // If we have a custom status without forced continue OR we have a status that is not NONE or SUCCESS or CUSTOM
            if ((data.Status == BotStatus.CUSTOM && !data.ConfigSettings.ContinueOnCustom) ||
                (data.Status != BotStatus.NONE && data.Status != BotStatus.SUCCESS && data.Status != BotStatus.CUSTOM))
            {
                i = lines.Count(); // Go to the end
                return;
            }

            TAKELINE:

            CurrentLine = lines[i];

            // Skip comments and blank lines
            if (IsEmptyOrCommentOrDisabled(CurrentLine))
            {
                i++; // Go to the next
                goto TAKELINE;
            }

            // Lookahead to compact lines. We don't use CompressedLines to be able to provide the line number for errors
            var lookahead = 0;

            // Join the line with the following ones if it's indented
            while (i + 1 + lookahead < lines.Count())
            {
                var nextLine = lines[i + 1 + lookahead];
                if (nextLine.StartsWith(" ") || nextLine.StartsWith("\t"))
                    CurrentLine += $" {nextLine.Trim()}";
                else break;

                lookahead++;
            }

            try
            {
                // If Block -> Process Block
                if (BlockParser.IsBlock(CurrentLine))
                {
                    BlockBase block = null;
                    try
                    {
                        block = BlockParser.Parse(CurrentLine);
                        CurrentBlock = block.Label;
                        if (!block.Disabled)
                            block.Process(data);
                    }
                    catch (Exception ex)
                    {
                        // We log the error message
                        data.LogBuffer.Add(new LogEntry("ERROR: " + ex.Message, Colors.Tomato));

                        // Stop the execution only if the block is vital for the execution of the script (requests)
                        // This way we prevent the interruption of the script and an endless retry cycle e.g. if we fail to parse a response given a specific input
                        if (block != null && (
                            block.GetType() == typeof(BlockRequest) ||
                            block.GetType() == typeof(BlockBypassCF) ||
                            block.GetType() == typeof(BlockImageCaptcha) ||
                            block.GetType() == typeof(BlockRecaptcha)))
                        {
                            data.Status = BotStatus.ERROR;
                            throw new BlockProcessingException(ex.Message);
                        }
                    }
                }

                // If Command -> Process Command
                else if (CommandParser.IsCommand(CurrentLine))
                {
                    try
                    {
                        var action = CommandParser.Parse(CurrentLine, data);
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        data.LogBuffer.Add(new LogEntry("ERROR: " + ex.Message, Colors.Tomato));
                        data.Status = BotStatus.ERROR;
                    }
                }

                // Try to Process Flow Control
                else
                {
                    var cfLine = CurrentLine;
                    var token = LineParser.ParseToken(ref cfLine, TokenType.Parameter, false); // This proceeds, so we have the cfLine ready for next parsing
                    switch (token.ToUpper())
                    {
                        case "IF":
                            // Check condition, if not true jump to line after first ELSE or ENDIF (check both at the same time on lines, not separately)
                            if (!ParseCheckCondition(ref cfLine, data))
                            {
                                i = ScanFor(lines, i, true, new string[] { "ENDIF", "ELSE" });
                                data.LogBuffer.Add(new LogEntry($"Jumping to line {i + 1}", Colors.White));
                            }
                            break;

                        case "ELSE":
                            // Here jump to ENDIF because you are coming from an IF and you don't need to process the ELSE
                            i = ScanFor(lines, i, true, new string[] { "ENDIF" });
                            data.LogBuffer.Add(new LogEntry($"Jumping to line {i + 1}", Colors.White));
                            break;

                        case "ENDIF":
                            break;

                        case "WHILE":
                            // Check condition, if false jump to first index after ENDWHILE
                            if (!ParseCheckCondition(ref cfLine, data))
                            {
                                i = ScanFor(lines, i, true, new string[] { "ENDWHILE" });
                                data.LogBuffer.Add(new LogEntry($"Jumping to line {i + 1}", Colors.White));
                            }
                            break;

                        case "ENDWHILE":
                            // Jump back to the previous WHILE index
                            i = ScanFor(lines, i, false, new string[] { "WHILE" }) - 1;
                            data.LogBuffer.Add(new LogEntry($"Jumping to line {i + 1}", Colors.White));
                            break;

                        case "JUMP":
                            var label = "";
                            try
                            {
                                label = LineParser.ParseToken(ref cfLine, TokenType.Label, true);
                                i = ScanFor(lines, -1, true, new string[] { $"{label}" }) - 1;
                                data.LogBuffer.Add(new LogEntry($"Jumping to line {i + 2}", Colors.White));
                            }
                            catch { throw new Exception($"No block with label {label} was found"); }
                            break;

                        case "BEGIN":
                            var beginToken = LineParser.ParseToken(ref cfLine, TokenType.Parameter, true);
                            switch (beginToken.ToUpper())
                            {
                                case "SCRIPT":
                                    language = (ScriptingLanguage)LineParser.ParseEnum(ref cfLine, "LANGUAGE", typeof(ScriptingLanguage));
                                    int end = 0;
                                    try
                                    {
                                        end = ScanFor(lines, i, true, new string[] { "END" }) - 1;
                                    }
                                    catch { throw new Exception("No 'END SCRIPT' specified"); }

                                    otherScript = string.Join(Environment.NewLine, lines.Skip(i + 1).Take(end - i));
                                    i = end;
                                    data.LogBuffer.Add(new LogEntry($"Jumping to line {i + 2}", Colors.White));
                                    break;
                            }
                            break;

                        case "END":
                            var endToken = LineParser.ParseToken(ref cfLine, TokenType.Parameter, true);
                            switch (endToken.ToUpper())
                            {
                                case "SCRIPT":
                                    LineParser.EnsureIdentifier(ref cfLine, "->");
                                    LineParser.EnsureIdentifier(ref cfLine, "VARS");
                                    var outputs = LineParser.ParseLiteral(ref cfLine, "OUTPUTS");

                                    try
                                    {
                                        if (otherScript != string.Empty) RunScript(otherScript, language, outputs, data);
                                    }
                                    catch (Exception ex) { data.LogBuffer.Add(new LogEntry($"The script failed to be executed: {ex.Message}", Colors.Tomato)); }
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (BlockProcessingException) { throw; } // Rethrow the Block Processing Exception so the error can be displayed in the view above
            catch (Exception e) { throw new Exception($"Parsing Exception on line {i + 1}: {e.Message}"); } // Catch inner and throw line exception

            i += 1 + lookahead;
        }

        /// <summary>
        /// Tests if a line is empty, a comment (starting with ##) or if it's disabled (starting with !).
        /// </summary>
        /// <param name="line">The line to test.</param>
        /// <returns>Whether the line needs to be skipped</returns>
        private static bool IsEmptyOrCommentOrDisabled(string line)
        {
            try { return line.Trim() == string.Empty || line.StartsWith("##") || line.StartsWith("!"); }
            catch { return true; }
        }

        /// <summary>
        /// Scans for a given set of identifiers in the script and returns the line index value of the first found.
        /// </summary>
        /// <param name="lines">The lines of the script</param>
        /// <param name="current">The index of the current line</param>
        /// <param name="downwards">Whether to scan downwards or upwards</param>
        /// <param name="options">The target identifiers</param>
        /// <returns></returns>
        public static int ScanFor(string[] lines, int current, bool downwards, string[] options)
        {
            var i = downwards ? current + 1 : current - 1;
            var found = false;
            while (i >= 0 && i < lines.Count())
            {
                try
                {
                    var token = LineParser.ParseToken(ref lines[i], TokenType.Parameter, false, false);
                    if (options.Any(o => token.ToUpper() == o.ToUpper()))
                    {
                        found = true;
                        break;
                    }
                }
                catch { }

                if (downwards) i++;
                else i--;
            }

            if (found) return i;
            else throw new Exception("Not found");
        }

        /// <summary>
        /// Parses a condition made of left-hand term, condition type and right-hand term and verifies if it's true.
        /// </summary>
        /// <param name="cfLine">The reference to the line to parse</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns></returns>
        public static bool ParseCheckCondition(ref string cfLine, BotData data)
        {
            var first = LineParser.ParseLiteral(ref cfLine, "STRING");
            var Comparer = (Comparer)LineParser.ParseEnum(ref cfLine, "Comparer", typeof(Comparer));
            var second = "";
            if (Comparer != Comparer.Exists && Comparer != Comparer.DoesNotExist)
                second = LineParser.ParseLiteral(ref cfLine, "STRING");
            return (Condition.ReplaceAndVerify(first, Comparer, second, data));
        }

        /// <summary>
        /// Runs a script with a different language inside the LoliScript.
        /// </summary>
        /// <param name="script">The script as a string with linebreaks</param>
        /// <param name="language">The language of the script</param>
        /// <param name="outputs">The variables that should be extracted from the script's scope and set into the BotData local variables</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        private void RunScript(string script, ScriptingLanguage language, string outputs, BotData data)
        {
            // Set the console output to stringwriter
            var sw = new StringWriter();
            Console.SetOut(sw);
            Console.SetError(sw);

            // Parse variables to get out
            List<string> outVarList = new List<string>();
            if (outputs != string.Empty)
            {
                try { outVarList = outputs.Split(',').Select(x => x.Trim()).ToList(); }
                catch { }
            }

            var start = DateTime.Now;

            try
            {
                switch (language)
                {
                    case ScriptingLanguage.JavaScript:

                        // Redefine log() function
                        var jsengine = new Engine().SetValue("log", new Action<object>(Console.WriteLine));

                        // Add in all the variables
                        foreach (var variable in data.Variables.All)
                        {
                            try
                            {
                                switch (variable.Type)
                                {
                                    case CVar.VarType.List:
                                        jsengine.SetValue(variable.Name, (variable.Value as List<string>).ToArray());
                                        break;

                                    default:
                                        jsengine.SetValue(variable.Name, variable.Value.ToString());
                                        break;
                                }
                            }
                            catch { }
                        }

                        // Execute JS
                        jsengine.Execute(script);

                        // Print results to log
                        data.Log(new LogEntry($"DEBUG LOG: {sw.ToString()}", Colors.White));

                        // Get variables out
                        data.Log(new LogEntry($"Parsing {outVarList.Count} variables", Colors.White));
                        foreach (var name in outVarList)
                        {
                            try
                            {
                                // Add it to the variables and print info
                                var value = jsengine.Global.GetProperty(name).Value;
                                var isArray = value.IsArray();
                                if (isArray) data.Variables.Set(new CVar(name, CVar.VarType.List, value.TryCast<List<string>>()));
                                else data.Variables.Set(new CVar(name, CVar.VarType.Single, value.ToString()));
                                data.Log(new LogEntry($"SET VARIABLE {name} WITH VALUE {value.ToString()}", Colors.Yellow));
                            }
                            catch { data.Log(new LogEntry($"COULD NOT FIND VARIABLE {name}", Colors.Tomato)); }
                        }

                        // Print other info
                        if (jsengine.GetCompletionValue() != null)
                            data.Log(new LogEntry($"Completion value: {jsengine.GetCompletionValue()}", Colors.White));

                        break;

                    case ScriptingLanguage.IronPython:

                        // Initialize the engine
                        var runtime = Python.CreateRuntime();
                        var pyengine = runtime.GetEngine("py");
                        PythonCompilerOptions pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
                        pco.Module &= ~ModuleOptions.Optimized;
                        //var pyengine = Python.CreateEngine();
                        var scope = pyengine.CreateScope();
                        var code = pyengine.CreateScriptSourceFromString(script);

                        // Add in all the variables
                        foreach (var variable in data.Variables.All)
                            try { scope.SetVariable(variable.Name, variable.Value); } catch { }

                        // Execute it
                        var result = code.Execute(scope);
                        //var result = pyengine.Execute(script, scope);

                        // Print the logs
                        data.Log(new LogEntry($"DEBUG LOG: {sw.ToString()}", Colors.White));

                        // Get variables out
                        data.Log(new LogEntry($"Parsing {outVarList.Count} variables", Colors.White));
                        foreach (var name in outVarList)
                        {
                            try
                            {
                                // Add it to the variables and print info
                                var value = scope.GetVariable(name);
                                data.Variables.Set(new CVar(name, (value.GetType() == typeof(string[])) ? CVar.VarType.List : CVar.VarType.Single, value.ToString()));
                                data.Log(new LogEntry($"SET VARIABLE {name} WITH VALUE {value}", Colors.Yellow));
                            }
                            catch { data.Log(new LogEntry($"COULD NOT FIND VARIABLE {name}", Colors.Tomato)); }
                        }

                        // Print other info
                        if (result != null)
                            data.Log(new LogEntry($"Completion value: {result}", Colors.White));

                        break;

                    default:
                        break;
                }

                data.Log(new LogEntry($"Execution completed in {(DateTime.Now - start).TotalSeconds} seconds", Colors.GreenYellow));
            }
            catch (Exception e)
            {
                data.Log(new LogEntry($"[ERROR] INFO: {e.Message}", Colors.White));
            }
            finally
            {
                var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                var standardError = new StreamWriter(Console.OpenStandardError());
                standardOutput.AutoFlush = true;
                standardError.AutoFlush = true;
                Console.SetOut(standardOutput);
                Console.SetError(standardError);
            }
        }

        /// <summary>Returns a list of all lines of the script, where expanded blocks have been compressed into one-liners.</summary>
        private string[] GetCompressedLines()
        {
            var i = 0;
            var isScript = false;
            var compressed = Script.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            while (i < compressed.Count - 1)
            {
                if (!isScript && BlockParser.IsBlock(compressed[i]) && (compressed[i + 1].StartsWith(" ") || compressed[i + 1].StartsWith("\t")))
                {
                    compressed[i] += $" {compressed[i + 1].Trim()}";
                    compressed.RemoveAt(i + 1);
                }
                else if (!isScript && BlockParser.IsBlock(compressed[i]) && (compressed[i + 1].StartsWith("! ") || compressed[i + 1].StartsWith("!\t")))
                {
                    compressed[i] += $" {compressed[i + 1].Substring(1).Trim()}";
                    compressed.RemoveAt(i + 1);
                }
                else
                {
                    if (compressed[i].StartsWith("BEGIN SCRIPT")) isScript = true;
                    else if (compressed[i].StartsWith("END SCRIPT")) isScript = false;

                    i++;
                }
            }
            return compressed.ToArray();
        }
    }

    public enum ScriptingLanguage
    {
        JavaScript,
        IronPython
    }
}
