using RuriLib.Legacy.Functions.Conditions;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that changes the bot status according to some conditions.
    /// </summary>
    public class BlockKeycheck : BlockBase
    {
        /// <summary>Whether to set the bot status to BAN if the last HTTP code was of type 4.</summary>
        public bool BanOn4XX { get; set; } = false;

        /// <summary>Whether to set the bot status as BAN if no keychain was valid.</summary>
        public bool BanOnToCheck { get; set; } = true;

        /// <summary>The list of all keychains.</summary>
        public List<KeyChain> KeyChains = new();

        /// <summary>
        /// Creates a KeyCheck block.
        /// </summary>
        public BlockKeycheck()
        {
            Label = "KEY CHECK";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax:
             * KEYCHECK [BAN4XX? BANTOCHECK?] [KEYCHAIN TYPE [CUSTOMNAME] MODE [KEY "STRING" [CONDITION "STRING"]]*]*
             * */

            while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                LineParser.SetBool(ref input, this);

            while (input != string.Empty) // Scan for keychains until the end
            {
                LineParser.EnsureIdentifier(ref input, "KEYCHAIN");

                KeyChain kc = new KeyChain { Type = (KeychainType)LineParser.ParseEnum(ref input, "Keychain Type", typeof(KeychainType)) };

                if (kc.Type == KeychainType.Custom && LineParser.Lookahead(ref input) == TokenType.Literal)
                    kc.CustomType = LineParser.ParseLiteral(ref input, "Custom Type");
                kc.Mode = (KeychainMode)LineParser.ParseEnum(ref input, "Keychain Mode", typeof(KeychainMode));

                while (!input.StartsWith("KEYCHAIN") && input != string.Empty) // Scan for keys
                {
                    var k = new Key();
                    LineParser.EnsureIdentifier(ref input, "KEY");

                    var first = LineParser.ParseLiteral(ref input, "Left Term");
                    // If there is another key/keychain/nothing, the user used the short syntax
                    if (LineParser.CheckIdentifier(ref input, "KEY") || LineParser.CheckIdentifier(ref input, "KEYCHAIN") || input == string.Empty)
                    {
                        k.LeftTerm = "<SOURCE>";
                        k.Comparer = Comparer.Contains;
                        k.RightTerm = first;
                    }
                    // Otherwise use the long syntax
                    else
                    {
                        k.LeftTerm = first;
                        k.Comparer = (Comparer)LineParser.ParseEnum(ref input, "Condition", typeof(Comparer));
                        if (k.Comparer != Comparer.Exists && k.Comparer != Comparer.DoesNotExist)
                            k.RightTerm = LineParser.ParseLiteral(ref input, "Right Term");
                    }

                    kc.Keys.Add(k);
                }

                KeyChains.Add(kc);
            }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("KEYCHECK")
                .Boolean(BanOn4XX, "BanOn4XX")
                .Boolean(BanOnToCheck, "BanOnToCheck");

            foreach(var kc in KeyChains)
            {
                writer
                    .Indent(1)
                    .Token("KEYCHAIN")
                    .Token(kc.Type);

                if (kc.Type == KeychainType.Custom)
                    writer.Literal(kc.CustomType);

                writer.Token(kc.Mode);

                foreach(var k in kc.Keys)
                {
                    if (k.LeftTerm == "<SOURCE>" && k.Comparer == Comparer.Contains)
                    {
                        writer
                            .Indent(2)
                            .Token("KEY")
                            .Literal(k.RightTerm);
                    }
                    else
                    {
                        writer
                        .Indent(2)
                        .Token("KEY")
                        .Literal(k.LeftTerm)
                        .Token(k.Comparer);

                        if (k.Comparer != Comparer.Exists && k.Comparer != Comparer.DoesNotExist)
                            writer.Literal(k.RightTerm);
                    }
                }
            }

            return writer.ToString();
        }

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            if (data.RESPONSECODE % 100 == 4 && BanOn4XX)
            {
                data.STATUS = "BAN";
                return;
            }

            // Check Global Keys
            if (data.Providers.ProxySettings.ContainsBanKey(data.SOURCE, out string _))
            {
                data.STATUS = "BAN";
                return;
            }

            // Check Retry Keys
            if (data.Providers.ProxySettings.ContainsRetryKey(data.SOURCE, out string _))
            {
                data.STATUS = "RETRY";
                return;
            }

            // KEYS WILL BE CHECKED IN THE ORDER IN WHICH THEY APPEAR IN THE LIST, THE LAST KEY HAS ABSOLUTE PRIORITY
            var found = false;
            foreach (var keychain in KeyChains)
            {
                if (keychain.CheckKeys(ls))
                {
                    found = true;
                    switch (keychain.Type)
                    {
                        case KeychainType.Success:
                            data.STATUS = "SUCCESS";
                            break;

                        case KeychainType.Failure:
                            data.STATUS = "FAIL";
                            break;

                        case KeychainType.Ban:
                            data.STATUS = "BAN";
                            break;

                        case KeychainType.Retry:
                            data.STATUS = "RETRY";
                            break;

                        case KeychainType.Custom:
                            data.STATUS = keychain.CustomType;
                            break;
                    }
                }
            }

            // If at the end no key was found, set it to ban
            if (!found && BanOnToCheck)
            {
                data.STATUS = "BAN";
            }
        }
    }
}
