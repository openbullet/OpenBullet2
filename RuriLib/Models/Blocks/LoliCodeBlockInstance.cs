using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Configs;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks
{
    public class LoliCodeBlockInstance : BlockInstance
    {
        private readonly string validTokenRegex = "[A-Za-z][A-Za-z0-9_]*";
        public string Script { get; set; }
        
        public LoliCodeBlockInstance(LoliCodeBlockDescriptor descriptor)
            : base(descriptor)
        {
            
        }

        public override string ToLC(bool printDefaultParams = false) => Script;

        public override void FromLC(ref string script, ref int lineNumber) 
        {
            Script = script;
            lineNumber += script.CountLines();
        }

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var reader = new StringReader(Script);
            using var writer = new StringWriter();
            string line, trimmedLine;

            while ((line = reader.ReadLine()) != null)
            {
                trimmedLine = line.Trim();

                // Try to read it as a LoliCode-exclusive statement
                try
                {
                    writer.WriteLine(TranspileStatement(trimmedLine, definedVariables));
                }

                // If it failed, we assume what is written is bare C# so we just copy it over (untrimmed)
                catch (NotSupportedException)
                {
                    writer.WriteLine(line);
                }
            }

            return writer.ToString();
        }

        private string TranspileStatement(string input, List<string> definedVariables)
        {
            Match match;

            // (RESOURCES) TAKEONE
            // TAKEONE FROM "MyResource" => "myString"
            if ((match = Regex.Match(input, "TAKEONE FROM (\"[^\"]+\") => @?\"?([^\"]+)\"?")).Success)
            {
                return $"string {match.Groups[2].Value} = globals.Resources[{match.Groups[1].Value}].TakeOne();";
            }

            // (RESOURCES) TAKE
            // TAKE 5 FROM "MyResource" => "myList"
            if ((match = Regex.Match(input, "TAKE ([0-9]+) FROM (\"[^\"]+\") => @?\"?([^\"]+)\"?")).Success)
            {
                return $"List<string> {match.Groups[3].Value} = globals.Resources[{match.Groups[2].Value}].Take({match.Groups[1].Value});";
            }

            // CODE LABEL
            // #MYLABEL => MYLABEL:
            if ((match = Regex.Match(input, $"^#({validTokenRegex})$")).Success)
            {
                return $"{match.Groups[1].Value}:";
            }

            // JUMP
            // JUMP #MYLABEL => goto MYLABEL;
            if ((match = Regex.Match(input, $"^JUMP #({validTokenRegex})$")).Success)
            {
                return $"goto {match.Groups[1].Value};";
            }

            // END
            // END => }
            if (input == "END")
            {
                return "}";
            }

            // REPEAT
            // REPEAT 10 => for (int xyz = 0; xyz < 10; xyz++) {
            if ((match = Regex.Match(input, $"^REPEAT (.+)$")).Success)
            {
                var i = VariableNames.RandomName();
                return $"for (var {i} = 0; {i} < ({match.Groups[1].Value}).AsInt(); {i}++){System.Environment.NewLine}{{";
            }

            // FOREACH
            // FOREACH v IN list => foreach (var v in list) {
            if ((match = Regex.Match(input, $"^FOREACH ({validTokenRegex}) IN ({validTokenRegex})$")).Success)
            {
                return $"foreach (var {match.Groups[1].Value} in {match.Groups[2].Value}){System.Environment.NewLine}{{";
            }

            // LOG
            // LOG myVar => data.Logger.Log(myVar);
            if ((match = Regex.Match(input, $"^LOG (.+)$")).Success)
            {
                return $"data.Logger.LogObject({match.Groups[1].Value});";
            }

            // CLOG
            // CLOG Tomato "hello" => data.Logger.Log("hello", LogColors.Tomato);
            if ((match = Regex.Match(input, $"^CLOG ([A-Za-z]+) (.+)$")).Success)
            {
                return $"data.Logger.LogObject({match.Groups[2].Value}, LogColors.{match.Groups[1].Value});";
            }

            // WHILE
            // WHILE a < b => while (a < b) {
            if ((match = Regex.Match(input, $"^WHILE (.+)$")).Success)
            {
                var line = match.Groups[1].Value.Trim();
                if (LoliCodeParser.keyIdentifiers.Any(t => line.StartsWith(t)))
                {
                    var keyType = LineParser.ParseToken(ref line);
                    var key = LoliCodeParser.ParseKey(ref line, keyType);
                    return $"while ({CSharpWriter.ConvertKey(key)}){System.Environment.NewLine}{{";
                }
                else
                {
                    return $"while ({line}){System.Environment.NewLine}{{";
                }
            }

            // IF
            // IF a < b => if (a < b) {
            if ((match = Regex.Match(input, $"^IF (.+)$")).Success)
            {
                var line = match.Groups[1].Value.Trim();
                if (LoliCodeParser.keyIdentifiers.Any(t => line.StartsWith(t)))
                {
                    var keyType = LineParser.ParseToken(ref line);
                    var key = LoliCodeParser.ParseKey(ref line, keyType);
                    return $"if ({CSharpWriter.ConvertKey(key)}){System.Environment.NewLine}{{";
                }
                else
                {
                    return $"if ({line}){System.Environment.NewLine}{{";
                }
            }

            // ELSE
            // ELSE => } else {
            if (input == "ELSE")
            {
                return $"}}{System.Environment.NewLine}else{System.Environment.NewLine}{{";
            }

            // ELSE IF
            // ELSE IF a < b => } else if (a < b) {
            if ((match = Regex.Match(input, $"ELSE IF (.+)$")).Success)
            {
                var line = match.Groups[1].Value.Trim();
                if (LoliCodeParser.keyIdentifiers.Any(t => line.StartsWith(t)))
                {
                    var keyType = LineParser.ParseToken(ref line);
                    var key = LoliCodeParser.ParseKey(ref line, keyType);
                    return $"}}{System.Environment.NewLine}else if ({CSharpWriter.ConvertKey(key)}){System.Environment.NewLine}{{";
                }
                else
                {
                    return $"}}{System.Environment.NewLine}else if ({line}){System.Environment.NewLine}{{";
                }
            }

            // TRY
            // TRY => try {
            if (input == "TRY")
            {
                return $"try{System.Environment.NewLine}{{";
            }

            // CATCH
            // CATCH => } catch {
            if (input == "CATCH")
            {
                return $"}}{System.Environment.NewLine}catch{System.Environment.NewLine}{{";
            }

            // FINALLY
            // FINALLY => } finally {
            if (input == "FINALLY")
            {
                return $"}}{System.Environment.NewLine}finally{System.Environment.NewLine}{{";
            }

            // LOCK
            // LOCK globals => lock (globals) {
            if ((match = Regex.Match(input, $"^LOCK (.+)$")).Success)
            {
                return $"lock({match.Groups[1].Value}){System.Environment.NewLine}{{";
            }

            // ACQUIRELOCK
            // ACQUIRELOCK globals => await data.AsyncLocker.Acquire(nameof(globals), data.CancellationToken);
            if ((match = Regex.Match(input, $"^ACQUIRELOCK (.+)$")).Success)
            {
                return $"await data.AsyncLocker.Acquire(nameof({match.Groups[1].Value}), data.CancellationToken);";
            }

            // RELEASELOCK
            // RELEASELOCK globals => data.AsyncLocker.Release(nameof(globals));
            if ((match = Regex.Match(input, $"^RELEASELOCK (.+)$")).Success)
            {
                return $"data.AsyncLocker.Release(nameof({match.Groups[1].Value}));";
            }

            // SET VAR
            // SET VAR myString "hello" => string myString = "hello";
            if ((match = Regex.Match(input, $"^SET VAR @?\"?({validTokenRegex})\"? (.+)$")).Success)
            {
                if (definedVariables.Contains(match.Groups[1].Value))
                {
                    return $"{match.Groups[1].Value} = {match.Groups[2].Value};";
                }
                else
                {
                    definedVariables.Add(match.Groups[1].Value);
                    return $"string {match.Groups[1].Value} = {match.Groups[2].Value};";
                }
            }

            // SET CAP
            // SET CAP myCapture "hello" => string myString = "hello"; data.MarkForCapture(nameof(myCapture));
            if ((match = Regex.Match(input, $"^SET CAP @?\"?({validTokenRegex})\"? (.+)$")).Success)
            {
                if (definedVariables.Contains(match.Groups[1].Value))
                {
                    return $"{match.Groups[1].Value} = {match.Groups[2].Value};{System.Environment.NewLine}data.MarkForCapture(nameof({match.Groups[1].Value}));";
                }
                else
                {
                    definedVariables.Add(match.Groups[1].Value);
                    return $"string {match.Groups[1].Value} = {match.Groups[2].Value};{System.Environment.NewLine}data.MarkForCapture(nameof({match.Groups[1].Value}));";
                }
            }

            // SET USEPROXY
            // SET USEPROXY TRUE => data.UseProxy = "true";
            if ((match = Regex.Match(input, "^SET USEPROXY (TRUE|FALSE)$")).Success)
            {
                return $"data.UseProxy = {match.Groups[1].Value.ToLower()};";
            }

            // SET PROXY
            // SET PROXY "127.0.0.1" 9050 SOCKS5 => data.Proxy = new Proxy("127.0.0.1", 9050, ProxyType.Socks5);
            // SET PROXY "127.0.0.1" 9050 SOCKS5 "username" "password" => data.Proxy = new Proxy("127.0.0.1", 9050, ProxyType.Socks5, "username", "password");
            if (input.StartsWith("SET PROXY "))
            {
                var setProxyParams = input["SET PROXY ".Length..].Split(' ');
                var proxyType = (ProxyType)Enum.Parse(typeof(ProxyType), setProxyParams[2], true);

                if (setProxyParams.Length == 3)
                {
                    return $"data.Proxy = new Proxy({setProxyParams[0]}, {setProxyParams[1]}, ProxyType.{proxyType});";
                }
                else
                {
                    return $"data.Proxy = new Proxy({setProxyParams[0]}, {setProxyParams[1]}, ProxyType.{proxyType}, {setProxyParams[3]}, {setProxyParams[4]});";
                }
            }

            // MARK
            // MARK @myVar => data.MarkForCapture(nameof(myVar));
            if ((match = Regex.Match(input, $"^MARK @?({validTokenRegex})$")).Success)
            {
                return $"data.MarkForCapture(nameof({match.Groups[1].Value}));";
            }

            // UNMARK
            // UNMARK @myVar => data.MarkedForCapture.Remove(nameof(myVar));
            if ((match = Regex.Match(input, $"^UNMARK @?({validTokenRegex})$")).Success)
            {
                return $"data.UnmarkCapture(nameof({match.Groups[1].Value}));";
            }

            throw new NotSupportedException();
        }
    }
}
