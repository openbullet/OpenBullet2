using RuriLib.Functions.Conditions;
using RuriLib.LS;
using RuriLib.Models;
using System;
using System.Collections.Generic;

namespace RuriLib
{
    /// <summary>
    /// A block that changes the bot status according to some conditions.
    /// </summary>
    public class BlockKeycheck : BlockBase
    {
        private bool banOn4XX = false;
        /// <summary>Whether to set the bot status to BAN if the last HTTP code was of type 4.</summary>
        public bool BanOn4XX { get { return banOn4XX; } set { banOn4XX = value; OnPropertyChanged(); } }

        private bool banOnToCheck = true;
        /// <summary>Whether to set the bot status as BAN if no keychain was valid.</summary>
        public bool BanOnToCheck { get { return banOnToCheck; } set { banOnToCheck = value; OnPropertyChanged(); } }

        /// <summary>The list of all keychains.</summary>
        public List<KeyChain> KeyChains = new List<KeyChain>();        

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

                KeyChain kc = new KeyChain();

                kc.Type = (KeyChain.KeychainType)LineParser.ParseEnum(ref input, "Keychain Type", typeof(KeyChain.KeychainType));
                if (kc.Type == KeyChain.KeychainType.Custom && LineParser.Lookahead(ref input) == TokenType.Literal)
                    kc.CustomType = LineParser.ParseLiteral(ref input, "Custom Type");
                kc.Mode = (KeyChain.KeychainMode)LineParser.ParseEnum(ref input, "Keychain Mode", typeof(KeyChain.KeychainMode));

                while (!input.StartsWith("KEYCHAIN") && input != string.Empty) // Scan for keys
                {
                    Key k = new Key();
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

                if (kc.Type == KeyChain.KeychainType.Custom)
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
        public override void Process(BotData data)
        {   
            base.Process(data);

            if (data.ResponseCode.StartsWith("4") && banOn4XX)
            {
                data.Status = BotStatus.BAN;
                return;
            }

            // Check Global Keys
            foreach (var key in data.GlobalSettings.Proxies.GlobalBanKeys)
            {
                if (key != string.Empty && data.ResponseSource.Contains(key))
                {
                    data.Status = BotStatus.BAN;
                    return;
                }
            }

            // KEYS WILL BE CHECKED IN THE ORDER IN WHICH THEY APPEAR IN THE LIST, THE LAST KEY HAS ABSOLUTE PRIORITY
            bool found = false;
            foreach (var keychain in KeyChains)
            {
                if (keychain.CheckKeys(data))
                {
                    found = true;
                    switch (keychain.Type)
                    {
                        case KeyChain.KeychainType.Success:
                            data.Status = BotStatus.SUCCESS;
                            break;

                        case KeyChain.KeychainType.Failure:
                            data.Status = BotStatus.FAIL;
                            break;

                        case KeyChain.KeychainType.Ban:
                            data.Status = BotStatus.BAN;
                            break;

                        case KeyChain.KeychainType.Retry:
                            data.Status = BotStatus.RETRY;
                            break;

                        case KeyChain.KeychainType.Custom:
                            data.Status = BotStatus.CUSTOM;
                            data.CustomStatus = keychain.CustomType;
                            break;
                    }
                }
            }

            // If at the end no key was found, set it to ban
            if (!found && banOnToCheck)
                data.Status = BotStatus.BAN;
        }
    }
}
