using RuriLib.Functions.Conversion;
using RuriLib.Functions.Time;
using RuriLib.Legacy.Functions.Crypto;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Legacy.Blocks
{
    /// <summary>
    /// A block that can execute a specific function on one or multiple inputs.
    /// </summary>
    public class BlockFunction : BlockBase
    {
        /// <summary>
        /// The function name.
        /// </summary>
        public enum Function
        {
            /// <summary>Simply replaced the variables of the input.</summary>
            Constant,

            /// <summary>Encodes an input as a base64 string.</summary>
            Base64Encode,

            /// <summary>Decodes the string from a base64-encoded input.</summary>
            Base64Decode,

            /// <summary>Hashes an input string.</summary>
            Hash,

            /// <summary>Generates a HMAC for a given string.</summary>
            HMAC,

            /// <summary>Translates words in a given string.</summary>
            Translate,

            /// <summary>Converts a formatted date to a unix timestamp.</summary>
            DateToUnixTime,

            /// <summary>Gets the length of a string.</summary>
            Length,

            /// <summary>Converts all uppercase caracters in a string to lowercase.</summary>
            ToLowercase,

            /// <summary>Converts all lowercase characters in a string to uppercase.</summary>
            ToUppercase,

            /// <summary>Replaces some text with something else, with or without using regex.</summary>
            Replace,

            /// <summary>Gets the first match for a specific regex pattern.</summary>
            RegexMatch,

            /// <summary>Encodes the input to be used in a URL.</summary>
            URLEncode,

            /// <summary>Decodes a URL-encoded input.</summary>
            URLDecode,

            /// <summary>Unescapes characters in a string.</summary>
            Unescape,

            /// <summary>Encodes the input to be displayed in HTML or XML.</summary>
            HTMLEntityEncode,

            /// <summary>Decoded an input containing HTML or XML entities.</summary>
            HTMLEntityDecode,

            /// <summary>Converts a unix timestamp to a formatted date.</summary>
            UnixTimeToDate,

            /// <summary>Retrieves the current time as a unix timestamp.</summary>
            CurrentUnixTime,

            /// <summary>Converts a unix timestamp to the ISO8601 format.</summary>
            UnixTimeToISO8601,

            /// <summary>Generates a random integer.</summary>
            RandomNum,

            /// <summary>Generates a random string based on a mask.</summary>
            RandomString,

            /// <summary>Rounds a decimal input to the upper integer.</summary>
            Ceil,

            /// <summary>Rounds a decimal input to the lower integer.</summary>
            Floor,

            /// <summary>Rounds a decimal input to the nearest integer.</summary>
            Round,

            /// <summary>Computes mathematical operations between decimal numbers.</summary>
            Compute,

            /// <summary>Counts the occurrences of a string in another string.</summary>
            CountOccurrences,

            /// <summary>Clears the cookie jar used for HTTP requests.</summary>
            ClearCookies,

            /// <summary>Encrypts a string with RSA.</summary>
            RSAEncrypt,

            // <summary>Decrypts a string with RSA.</summary>
            // RSADecrypt,

            /// <summary>Encrypts a string with RSA PKCS1PAD2.</summary>
            RSAPKCS1PAD2,

            /// <summary>Waits a given amount of milliseconds.</summary>
            Delay,

            /// <summary>Retrieves the character at a given index in the input string.</summary>
            CharAt,

            /// <summary>Gets a substring of the input.</summary>
            Substring,

            /// <summary>Reverses the input string.</summary>
            ReverseString,

            /// <summary>Removes leading or trailing whitespaces from a string.</summary>
            Trim,

            /// <summary>Gets a valid random User-Agent header.</summary>
            GetRandomUA,

            /// <summary>Encrypts a string with AES.</summary>
            AESEncrypt,

            /// <summary>Decrypts an AES-encrypted string.</summary>
            AESDecrypt,

            /// <summary>Generates a key using a password based KDF.</summary>
            PBKDF2PKCS5
        }

        #region General Properties
        /// <summary>The name of the output variable.</summary>
        public string VariableName { get; set; } = "";

        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get; set; } = false;

        /// <summary>The input string on which the function will be executed (not always needed).</summary>
        public string InputString { get; set; } = "";

        /// <summary>The function to execute.</summary>
        public Function FunctionType { get; set; } = Function.Constant;
        #endregion

        #region Function Specific Properties
        // -- Hash & Hmac
        /// <summary>The hashing function to use.</summary>
        public Hash HashType { get; set; } = Hash.SHA512;

        /// <summary>Whether the input is a base64-encoded string instead of UTF8.</summary>
        public bool InputBase64 { get; set; } = false;

        // -- Hmac
        /// <summary>The key used to authenticate the message.</summary>
        public string HmacKey { get; set; } = "";

        /// <summary>Whether to output the message as a base64-encoded string instead of a hex-encoded string.</summary>
        public bool HmacBase64 { get; set; } = false;

        /// <summary>Whether the HMAC Key is a base64-encoded string instead of UTF8.</summary>
        public bool KeyBase64 { get; set; } = false;

        // -- Translate
        /// <summary>Whether to stop translating after the first match.</summary>
        public bool StopAfterFirstMatch { get; set; } = true;

        /// <summary>The dictionary containing the words and their translation.</summary>
        public Dictionary<string, string> TranslationDictionary { get; set; } = new Dictionary<string, string>();

        // -- Date to unix
        /// <summary>The format of the date (y = year, M = month, d = day, H = hour, m = minute, s = second).</summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd:HH-mm-ss";

        // -- string replace
        /// <summary>The text to replace.</summary>
        public string ReplaceWhat { get; set; } = "";

        /// <summary>The replacement text.</summary>
        public string ReplaceWith { get; set; } = "";

        /// <summary>Whether to use regex for replacing.</summary>
        public bool UseRegex { get; set; } = false;

        // -- Regex Match
        /// <summary>The regex pattern to match.</summary>
        public string RegexMatch { get; set; } = "";

        // -- Random Number
        /// <summary>The minimum random number that can be generated (inclusive).</summary>
        public string RandomMin { get; set; } = "0";

        /// <summary>The maximum random number that can be generated (exclusive).</summary>
        public string RandomMax { get; set; } = "0";

        /// <summary>Whether to pad with zeros on the left to match the length of the maximum provided.</summary>
        public bool RandomZeroPad { get; set; } = false;

        // -- CountOccurrences
        /// <summary>The string to count the occurrences of.</summary>
        public string StringToFind { get; set; } = "";

        // -- RSA
        /// <summary>The modulus of the RSA public key as a base64 string.</summary>
        public string RsaN { get; set; } = "";

        /// <summary>The exponent of the RSA public key as a base64 string.</summary>
        public string RsaE { get; set; } = "";

        /// <summary>The exponent of the RSA private key as a base64 string.</summary>
        public string RsaD { get; set; } = "";

        /// <summary>Whether to use OAEP padding instead of PKCS v1.5.</summary>
        public bool RsaOAEP { get; set; } = true;

        // --- CharAt
        /// <summary>The index of the wanted character.</summary>
        public string CharIndex { get; set; } = "0";

        // -- Substring
        /// <summary>The starting index for the substring.</summary>
        public string SubstringIndex { get; set; } = "0";

        /// <summary>The length of the wanted substring.</summary>
        public string SubstringLength { get; set; } = "1";

        // -- User Agent
        /// <summary>Whether to only limit the UA generation to a certain browser.</summary>
        public bool UserAgentSpecifyBrowser { get; set; } = false;

        /// <summary>The browser for which the User Agent should be generated.</summary>
        public Browser UserAgentBrowser { get; set; } = Browser.Chrome;

        // -- AES
        /// <summary>The keys used for AES encryption and decryption as a base64 string.</summary>
        public string AesKey { get; set; } = "";

        /// <summary>The initial value as a base64 string.</summary>
        public string AesIV { get; set; } = "";

        /// <summary>The cipher mode.</summary>
        public CipherMode AesMode { get; set; } = CipherMode.CBC;

        /// <summary>The padding mode.</summary>
        public PaddingMode AesPadding { get; set; } = PaddingMode.None;

        // -- PBKDF2PKCS5
        /// <summary>The KDF's salt as a base64 string.</summary>
        public string KdfSalt { get; set; } = "";

        /// <summary>The size of the generated salt (in bytes) in case none is specified.</summary>
        public int KdfSaltSize { get; set; } = 8;

        /// <summary>The number of times to perform the algorithm.</summary>
        public int KdfIterations { get; set; } = 1;

        /// <summary>The size of the generated key (in bytes).</summary>
        public int KdfKeySize { get; set; } = 16;

        /// <summary>The size of the generated salt (in bytes) in case none is specified.</summary>
        public Hash KdfAlgorithm { get; set; } = Hash.SHA1;
        #endregion

        #region RandomString Properties
        private static readonly string _lowercase = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string _uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string _digits = "0123456789";
        private static readonly string _symbols = "\\!\"£$%&/()=?^'{}[]@#,;.:-_*+";
        private static readonly string _hex = _digits + "abcdef";
        private static readonly string _udChars = _uppercase + _digits;
        private static readonly string _ldChars = _lowercase + _digits;
        private static readonly string _upperlwr = _lowercase + _uppercase;
        private static readonly string _ludChars = _lowercase + _uppercase + _digits;
        private static readonly string _allChars = _lowercase + _uppercase + _digits + _symbols;

        #endregion

        /// <summary>
        /// Creates a Function block.
        /// </summary>
        public BlockFunction()
        {
            Label = "FUNCTION";
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
             * FUNCTION Name [ARGUMENTS] ["INPUT STRING"] [-> VAR/CAP "NAME"]
             * */

            // Parse the function
            FunctionType = (Function)LineParser.ParseEnum(ref input, "Function Name", typeof(Function));

            // Parse specific function parameters
            switch (FunctionType)
            {
                case Function.Hash:
                    HashType = LineParser.ParseEnum(ref input, "Hash Type", typeof(Hash));
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.HMAC:
                    HashType = LineParser.ParseEnum(ref input, "Hash Type", typeof(Hash));
                    HmacKey = LineParser.ParseLiteral(ref input, "HMAC Key");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.Translate:
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    TranslationDictionary = new Dictionary<string, string>();
                    while (input != string.Empty && LineParser.Lookahead(ref input) == TokenType.Parameter)
                    {
                        LineParser.EnsureIdentifier(ref input, "KEY");
                        var k = LineParser.ParseLiteral(ref input, "Key");
                        LineParser.EnsureIdentifier(ref input, "VALUE");
                        var v = LineParser.ParseLiteral(ref input, "Value");
                        TranslationDictionary[k] = v;
                    }
                    break;

                case Function.DateToUnixTime:
                    DateFormat = LineParser.ParseLiteral(ref input, "DATE FORMAT");
                    break;

                case Function.UnixTimeToDate:
                    DateFormat = LineParser.ParseLiteral(ref input, "DATE FORMAT");
                    // a little backward compatability with the old line format.
                    if (LineParser.Lookahead(ref input) != TokenType.Literal)
                    {
                        InputString = DateFormat;
                        DateFormat = "yyyy-MM-dd:HH-mm-ss";
                    }
                    break;

                case Function.Replace:
                    ReplaceWhat = LineParser.ParseLiteral(ref input, "What");
                    ReplaceWith = LineParser.ParseLiteral(ref input, "With");
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.RegexMatch:
                    RegexMatch = LineParser.ParseLiteral(ref input, "Pattern");
                    break;

                case Function.RandomNum:
                    if (LineParser.Lookahead(ref input) == TokenType.Literal)
                    {
                        RandomMin = LineParser.ParseLiteral(ref input, "Minimum");
                        RandomMax = LineParser.ParseLiteral(ref input, "Maximum");
                    }
                    // Support for old integer definition of Min and Max
                    else
                    {
                        RandomMin = LineParser.ParseInt(ref input, "Minimum").ToString();
                        RandomMax = LineParser.ParseInt(ref input, "Maximum").ToString();
                    }

                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.CountOccurrences:
                    StringToFind = LineParser.ParseLiteral(ref input, "string to find");
                    break;

                case Function.CharAt:
                    CharIndex = LineParser.ParseLiteral(ref input, "Index");
                    break;

                case Function.Substring:
                    SubstringIndex = LineParser.ParseLiteral(ref input, "Index");
                    SubstringLength = LineParser.ParseLiteral(ref input, "Length");
                    break;

                case Function.RSAEncrypt:
                    RsaN = LineParser.ParseLiteral(ref input, "Public Key Modulus");
                    RsaE = LineParser.ParseLiteral(ref input, "Public Key Exponent");
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                /*
            case Function.RSADecrypt:
                RsaN = LineParser.ParseLiteral(ref input, "Public Key Modulus");
                RsaD = LineParser.ParseLiteral(ref input, "Private Key Exponent");
                if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                    LineParser.SetBool(ref input, this);
                break;
                */

                case Function.RSAPKCS1PAD2:
                    RsaN = LineParser.ParseLiteral(ref input, "Public Key Modulus");
                    RsaE = LineParser.ParseLiteral(ref input, "Public Key Exponent");
                    break;

                case Function.GetRandomUA:
                    if (LineParser.ParseToken(ref input, TokenType.Parameter, false, false) == "BROWSER")
                    {
                        LineParser.EnsureIdentifier(ref input, "BROWSER");
                        UserAgentSpecifyBrowser = true;
                        UserAgentBrowser = LineParser.ParseEnum(ref input, "BROWSER", typeof(Browser));
                    };
                    break;

                case Function.AESDecrypt:
                case Function.AESEncrypt:
                    AesKey = LineParser.ParseLiteral(ref input, "Key");
                    AesIV = LineParser.ParseLiteral(ref input, "IV");
                    AesMode = LineParser.ParseEnum(ref input, "Cipher mode", typeof(CipherMode));
                    AesPadding = LineParser.ParseEnum(ref input, "Padding mode", typeof(PaddingMode));
                    break;

                case Function.PBKDF2PKCS5:
                    if (LineParser.Lookahead(ref input) == TokenType.Literal) KdfSalt = LineParser.ParseLiteral(ref input, "Salt");
                    else KdfSaltSize = LineParser.ParseInt(ref input, "Salt size");
                    KdfIterations = LineParser.ParseInt(ref input, "Iterations");
                    KdfKeySize = LineParser.ParseInt(ref input, "Key size");
                    KdfAlgorithm = LineParser.ParseEnum(ref input, "Algorithm", typeof(Hash));
                    break;

                default:
                    break;
            }

            // Try to parse the input string
            if (LineParser.Lookahead(ref input) == TokenType.Literal)
                InputString = LineParser.ParseLiteral(ref input, "INPUT");

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == string.Empty)
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("FUNCTION")
                .Token(FunctionType);

            switch (FunctionType)
            {
                case Function.Hash:
                    writer
                        .Token(HashType)
                        .Boolean(InputBase64, nameof(InputBase64));
                    break;

                case Function.HMAC:
                    writer
                        .Token(HashType)
                        .Literal(HmacKey)
                        .Boolean(InputBase64, nameof(InputBase64))
                        .Boolean(HmacBase64, nameof(HmacBase64))
                        .Boolean(KeyBase64, nameof(KeyBase64));
                    break;

                case Function.Translate:
                    writer
                        .Boolean(StopAfterFirstMatch, nameof(StopAfterFirstMatch));
                    foreach (var t in TranslationDictionary)
                        writer
                            .Indent()
                            .Token("KEY")
                            .Literal(t.Key)
                            .Token("VALUE")
                            .Literal(t.Value);

                    writer
                        .Indent();
                    break;

                case Function.UnixTimeToDate:
                case Function.DateToUnixTime:
                    writer
                        .Literal(DateFormat);
                    break;

                case Function.Replace:
                    writer
                        .Literal(ReplaceWhat)
                        .Literal(ReplaceWith)
                        .Boolean(UseRegex, nameof(UseRegex));
                    break;

                case Function.RegexMatch:
                    writer
                        .Literal(RegexMatch, nameof(RegexMatch));
                    break;

                case Function.RandomNum:
                    writer
                        .Literal(RandomMin)
                        .Literal(RandomMax)
                        .Boolean(RandomZeroPad, nameof(RandomZeroPad));
                    break;

                case Function.CountOccurrences:
                    writer
                        .Literal(StringToFind);
                    break;

                case Function.CharAt:
                    writer
                        .Literal(CharIndex);
                    break;

                case Function.Substring:
                    writer
                        .Literal(SubstringIndex)
                        .Literal(SubstringLength);
                    break;

                case Function.RSAEncrypt:
                    writer
                        .Literal(RsaN)
                        .Literal(RsaE)
                        .Boolean(RsaOAEP, nameof(RsaOAEP));
                    break;

                /*
            case Function.RSADecrypt:
                writer
                    .Literal(RsaN)
                    .Literal(RsaD)
                    .Boolean(RsaOAEP, "RsaOAEP");
                break;
                */

                case Function.RSAPKCS1PAD2:
                    writer
                        .Literal(RsaN)
                        .Literal(RsaE);
                    break;

                case Function.GetRandomUA:
                    if (UserAgentSpecifyBrowser)
                    {
                        writer
                            .Token("BROWSER")
                            .Token(UserAgentBrowser);
                    }
                    break;

                case Function.AESDecrypt:
                case Function.AESEncrypt:
                    writer
                        .Literal(AesKey)
                        .Literal(AesIV)
                        .Token(AesMode)
                        .Token(AesPadding);
                    break;

                case Function.PBKDF2PKCS5:
                    if (KdfSalt != string.Empty) writer.Literal(KdfSalt);
                    else writer.Integer(KdfSaltSize);
                    writer
                        .Integer(KdfIterations)
                        .Integer(KdfKeySize)
                        .Token(KdfAlgorithm);
                    break;

            }

            writer
                .Literal(InputString, "InputString");
            if (!writer.CheckDefault(VariableName, "VariableName"))
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName);

            return writer.ToString();
        }

        private static readonly NumberStyles _style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        private static readonly IFormatProvider _provider = new CultureInfo("en-US");

        /// <inheritdoc />
        public override async Task Process(LSGlobals ls)
        {
            var data = ls.BotData;
            await base.Process(ls);

            var localInputStrings = ReplaceValuesRecursive(InputString, ls);
            var outputs = new List<string>();

            for (var i = 0; i < localInputStrings.Count; i++)
            {
                var localInputString = localInputStrings[i];
                var outputString = "";

                switch (FunctionType)
                {
                    case Function.Constant:
                        outputString = localInputString;
                        break;

                    case Function.Base64Encode:
                        outputString = Base64Converter.ToBase64String(Encoding.UTF8.GetBytes(localInputString));
                        break;

                    case Function.Base64Decode:
                        outputString = Encoding.UTF8.GetString(Base64Converter.ToByteArray(localInputString));
                        break;

                    case Function.HTMLEntityEncode:
                        outputString = WebUtility.HtmlEncode(localInputString);
                        break;

                    case Function.HTMLEntityDecode:
                        outputString = WebUtility.HtmlDecode(localInputString);
                        break;

                    case Function.Hash:
                        outputString = GetHash(localInputString, HashType, InputBase64).ToLower();
                        break;

                    case Function.HMAC:
                        outputString = Hmac(localInputString, HashType, ReplaceValues(HmacKey, ls), InputBase64, KeyBase64, HmacBase64);
                        break;

                    case Function.Translate:
                        outputString = localInputString;
                        foreach (var entry in TranslationDictionary.OrderBy(e => e.Key.Length).Reverse())
                        {
                            if (outputString.Contains(entry.Key))
                            {
                                outputString = outputString.Replace(entry.Key, entry.Value);
                                if (StopAfterFirstMatch) break;
                            }
                        }
                        break;

                    case Function.DateToUnixTime:
                        outputString = localInputString.ToDateTime(DateFormat).ToUnixTime().ToString();
                        break;

                    case Function.Length:
                        outputString = localInputString.Length.ToString();
                        break;

                    case Function.ToLowercase:
                        outputString = localInputString.ToLower();
                        break;

                    case Function.ToUppercase:
                        outputString = localInputString.ToUpper();
                        break;

                    case Function.Replace:
                        outputString = UseRegex
                            ? Regex.Replace(localInputString, ReplaceValues(ReplaceWhat, ls), ReplaceValues(ReplaceWith, ls))
                            : localInputString.Replace(ReplaceValues(ReplaceWhat, ls), ReplaceValues(ReplaceWith, ls));
                        break;

                    case Function.RegexMatch:
                        outputString = Regex.Match(localInputString, ReplaceValues(RegexMatch, ls)).Value;
                        break;

                    case Function.Unescape:
                        outputString = Regex.Unescape(localInputString);
                        break;

                    case Function.URLEncode:
                        // The maximum allowed Uri size is 2083 characters, we use 2080 as a precaution
                        outputString = string.Join("", SplitInChunks(localInputString, 2080).Select(Uri.EscapeDataString));
                        break;

                    case Function.URLDecode:
                        outputString = Uri.UnescapeDataString(localInputString);
                        break;

                    case Function.UnixTimeToDate:
                        outputString = long.Parse(localInputString).ToDateTimeUtc().ToString(DateFormat);
                        break;

                    case Function.CurrentUnixTime:
                        outputString = DateTime.UtcNow.ToUnixTime().ToString();
                        break;

                    case Function.UnixTimeToISO8601:
                        outputString = long.Parse(localInputString).ToDateTimeUtc().ToISO8601();
                        break;

                    case Function.RandomNum:
                        var min = int.Parse(ReplaceValues(RandomMin, ls));
                        var max = int.Parse(ReplaceValues(RandomMax, ls));
                        var randomNumString = data.Random.Next(min, max).ToString();
                        outputString = RandomZeroPad ? randomNumString.PadLeft(max.ToString().Length, '0') : randomNumString;
                        break;

                    case Function.RandomString:
                        outputString = localInputString;
                        outputString = Regex.Replace(outputString, @"\?l", m => _lowercase[data.Random.Next(_lowercase.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?u", m => _uppercase[data.Random.Next(_uppercase.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?d", m => _digits[data.Random.Next(_digits.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?s", m => _symbols[data.Random.Next(_symbols.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?h", m => _hex[data.Random.Next(_hex.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?a", m => _allChars[data.Random.Next(_allChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?m", m => _udChars[data.Random.Next(_udChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?n", m => _ldChars[data.Random.Next(_ldChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?i", m => _ludChars[data.Random.Next(_ludChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?f", m => _upperlwr[data.Random.Next(_upperlwr.Length)].ToString());
                        break;

                    case Function.Ceil:
                        outputString = Math.Ceiling(decimal.Parse(localInputString, _style, _provider)).ToString();
                        break;

                    case Function.Floor:
                        outputString = Math.Floor(decimal.Parse(localInputString, _style, _provider)).ToString();
                        break;

                    case Function.Round:
                        outputString = Math.Round(decimal.Parse(localInputString, _style, _provider), 0, MidpointRounding.AwayFromZero).ToString();
                        break;

                    case Function.Compute:
                        outputString = new DataTable().Compute(localInputString.Replace(',', '.'), null).ToString();
                        break;

                    case Function.CountOccurrences:
                        outputString = CountStringOccurrences(localInputString, StringToFind).ToString();
                        break;

                    case Function.ClearCookies:
                        data.COOKIES.Clear();
                        break;

                    case Function.RSAEncrypt:
                        outputString = Crypto.RSAEncrypt(
                            localInputString,
                            ReplaceValues(RsaN, ls),
                            ReplaceValues(RsaE, ls),
                            RsaOAEP
                            );
                        break;

                    /*
                case Function.RSADecrypt:
                    outputString = Crypto.RSADecrypt(
                        localInputString,
                        ReplaceValues(RsaN, data),
                        ReplaceValues(RsaD, data),
                        RsaOAEP
                        );
                    break;
                    */

                    case Function.RSAPKCS1PAD2:
                        outputString = Crypto.RSAPkcs1Pad2(
                            localInputString,
                            ReplaceValues(RsaN, ls),
                            ReplaceValues(RsaE, ls)
                            );
                        break;

                    case Function.Delay:
                        try { Thread.Sleep(int.Parse(localInputString)); } catch { }
                        break;

                    case Function.CharAt:
                        outputString = localInputString.ToCharArray()[int.Parse(ReplaceValues(CharIndex, ls))].ToString();
                        break;

                    case Function.Substring:
                        outputString = localInputString.Substring(int.Parse(ReplaceValues(SubstringIndex, ls)), int.Parse(ReplaceValues(SubstringLength, ls)));
                        break;

                    case Function.ReverseString:
                        var charArray = localInputString.ToCharArray();
                        Array.Reverse(charArray);
                        outputString = new string(charArray);
                        break;

                    case Function.Trim:
                        outputString = localInputString.Trim();
                        break;

                    case Function.GetRandomUA:
                        outputString = data.Providers.RandomUA.Generate();
                        break;

                    case Function.AESEncrypt:
                        outputString = Crypto.AESEncrypt(localInputString, ReplaceValues(AesKey, ls),
                            ReplaceValues(AesIV, ls), AesMode, AesPadding);
                        break;

                    case Function.AESDecrypt:
                        outputString = Crypto.AESDecrypt(localInputString, ReplaceValues(AesKey, ls),
                            ReplaceValues(AesIV, ls), AesMode, AesPadding);
                        break;

                    case Function.PBKDF2PKCS5:
                        outputString = Crypto.PBKDF2PKCS5(localInputString, ReplaceValues(KdfSalt, ls),
                            KdfSaltSize, KdfIterations, KdfKeySize, KdfAlgorithm);
                        break;
                }

                data.Logger.Log($"Executed function {FunctionType} on input {localInputString} with outcome {outputString}", LogColors.GreenYellow);

                // Add to the outputs
                outputs.Add(outputString);
            }

            var isList = outputs.Count > 1 || InputString.Contains("[*]") || InputString.Contains("(*)") || InputString.Contains("{*}");
            InsertVariable(ls, IsCapture, isList, outputs, VariableName, "", "", false, true);
        }

        /// <summary>
        /// Hashes a string using the specified hashing function.
        /// </summary>
        /// <param name="baseString">The string to hash</param>
        /// <param name="type">The hashing function</param>
        /// <param name="inputBase64">Whether the base string should be treated as base64 encoded (if false, it will be treated as UTF8 encoded)</param>
        /// <returns>The hash digest as a hex-encoded uppercase string.</returns>
        public static string GetHash(string baseString, Hash type, bool inputBase64)
        {
            var rawInput = inputBase64 ? Convert.FromBase64String(baseString) : Encoding.UTF8.GetBytes(baseString);
            
            var digest = type switch
            {
                Hash.MD4 => Crypto.MD4(rawInput),
                Hash.MD5 => Crypto.MD5(rawInput),
                Hash.SHA1 => Crypto.SHA1(rawInput),
                Hash.SHA256 => Crypto.SHA256(rawInput),
                Hash.SHA384 => Crypto.SHA384(rawInput),
                Hash.SHA512 => Crypto.SHA512(rawInput),
                _ => throw new NotSupportedException("Unsupported algorithm"),
            };
            
            return HexConverter.ToHexString(digest);
        }

        /// <summary>
        /// Gets the HMAC signature of a message given a key and a hashing function.
        /// </summary>
        /// <param name="baseString">The message to sign</param>
        /// <param name="type">The hashing function</param>
        /// <param name="key">The HMAC key</param>
        /// <param name="inputBase64">Whether the input string should be treated as base64 encoded (if false, it will be treated as UTF8 encoded)</param>
        /// <param name="keyBase64">Whether the key string should be treated as base64 encoded (if false, it will be treated as UTF8 encoded)</param>
        /// <param name="outputBase64">Whether the output should be encrypted as a base64 string</param>
        /// <returns>The HMAC signature</returns>
        public static string Hmac(string baseString, Hash type, string key, bool inputBase64, bool keyBase64, bool outputBase64)
        {
            var rawInput = inputBase64 ? Convert.FromBase64String(baseString) : Encoding.UTF8.GetBytes(baseString);
            var rawKey = keyBase64 ? Convert.FromBase64String(key) : Encoding.UTF8.GetBytes(key);
            
            var signature = type switch
            {
                Hash.MD5 => Crypto.HMACMD5(rawInput, rawKey),
                Hash.SHA1 => Crypto.HMACSHA1(rawInput, rawKey),
                Hash.SHA256 => Crypto.HMACSHA256(rawInput, rawKey),
                Hash.SHA384 => Crypto.HMACSHA384(rawInput, rawKey),
                Hash.SHA512 => Crypto.HMACSHA512(rawInput, rawKey),
                _ => throw new NotSupportedException("Unsupported algorithm"),
            };

            return outputBase64 ? Base64Converter.ToBase64String(signature) : HexConverter.ToHexString(signature);
        }

        #region Translation

        /// <summary>
        /// Builds a string containing translation keys.
        /// </summary>
        /// <returns>One translation key per line, with name and value separated by a colon</returns>
        public string GetDictionary()
        {
            var sb = new StringBuilder();

            foreach (var pair in TranslationDictionary)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(TranslationDictionary.Last())) sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sets translation keys from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the translation keys</param>
        public void SetDictionary(string[] lines)
        {
            TranslationDictionary.Clear();

            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    var key = split[0];
                    var val = split[1].TrimStart();
                    TranslationDictionary[key] = val;
                }
            }
        }
        #endregion

        #region Count Occurrences
        /// <summary>
        /// Counts how many times a string occurs inside another string.
        /// </summary>
        /// <param name="input">The long string</param>
        /// <param name="text">The text to search</param>
        /// <returns>How many times the text appears in the long string</returns>
        public static int CountStringOccurrences(string input, string text)
        {
            // Loop through all instances of the string 'text'.
            var count = 0;
            var i = 0;

            while ((i = input.IndexOf(text, i)) != -1)
            {
                i += text.Length;
                count++;
            }

            return count;
        }
        #endregion

        #region Others
        /// <summary>
        /// Splits a string in chunks of a given size.
        /// </summary>
        /// <param name="str">The string to split</param>
        /// <param name="chunkSize">The maximum chunk size</param>
        /// <returns>An array of strings where the last one might be shorter than the maximum chunk size.</returns>
        public static string[] SplitInChunks(string str, int chunkSize)
        {
            if (str.Length < chunkSize) return new string[] { str };
            return Enumerable.Range(0, (int)Math.Ceiling(str.Length / (double)chunkSize))
                .Select(i => str.Substring(i * chunkSize, Math.Min(str.Length - i * chunkSize, chunkSize)))
                .ToArray();
        }
        #endregion

        /// <summary>
        /// Enumerates browsers for which a User Agent can be generated.
        /// </summary>
        public enum Browser
        {
            /// <summary>The Google Chrome browser.</summary>
            Chrome,

            /// <summary>The Mozilla Firefox browser.</summary>
            Firefox,

            /// <summary>The Internet Explorer browser.</summary>
            InternetExplorer,

            /// <summary>The Opera browser.</summary>
            Opera,

            /// <summary>The Opera Mini mobile browser.</summary>
            OperaMini
        }
    }
}
