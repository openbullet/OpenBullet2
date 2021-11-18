function registerLoliScript() {
    // Register a new language
    monaco.languages.register({ id: 'loliscript' });

    monaco.languages.setLanguageConfiguration('loliscript', {
        wordPattern: /(-?\d*\.\d\w*)|([^\`\~\!\#\$\%\^\&\*\(\)\-\=\+\[\{\]\}\\\|\;\:\'\"\,\.\<\>\/\?\s]+)/g,
        comments: {
            lineComment: "//",
            blockComment: ["/*", "*/"]
        },
        brackets: [
            ["{", "}"],
            ["[", "]"],
            ["(", ")"]
        ],
        autoClosingPairs: [{
            open: "{",
            close: "}"
        }, {
            open: "[",
            close: "]"
        }, {
            open: "(",
            close: ")"
        }, {
            open: "'",
            close: "'",
            notIn: ["string", "comment"]
        }, {
            open: '"',
            close: '"',
            notIn: ["string", "comment"]
        }],
        surroundingPairs: [{
            open: "{",
            close: "}"
        }, {
            open: "[",
            close: "]"
        }, {
            open: "(",
            close: ")"
        }, {
            open: "<",
            close: ">"
        }, {
            open: "'",
            close: "'"
        }, {
            open: '"',
            close: '"'
        }],
        folding: {
            markers: {
                start: new RegExp("^\\s*#region\\b"),
                end: new RegExp("^\\s*#endregion\\b")
            }
        }
    });

    // Register a tokens provider for the language
    monaco.languages.setMonarchTokensProvider('loliscript', {

        keywords: ["extern", "alias", "using", "bool", "decimal", "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "object", "dynamic", "string", "assembly", "is", "as", "ref", "out", "this", "base", "new", "typeof", "void", "checked", "unchecked", "default", "delegate", "var", "const", "if", "else", "switch", "case", "while", "do", "for", "foreach", "in", "break", "continue", "goto", "return", "throw", "try", "catch", "finally", "lock", "yield", "from", "let", "where", "join", "on", "equals", "into", "orderby", "ascending", "descending", "select", "group", "by", "namespace", "partial", "class", "field", "event", "method", "param", "property", "public", "protected", "internal", "private", "abstract", "sealed", "static", "struct", "readonly", "volatile", "virtual", "override", "params", "get", "set", "add", "remove", "operator", "true", "false", "implicit", "explicit", "interface", "enum", "null", "async", "await", "fixed", "sizeof", "stackalloc", "unsafe", "nameof", "when",
            "JUMP", "REPEAT", "END", "FOREACH", "IN", "LOG", "CLOG", "WHILE", "IF", "ELSE", "ELSE IF", "TRY", "CATCH", "LOCK", "SET", "TAKE", "TAKEONE", "FROM", "FINALLY", "ACQUIRELOCK", "RELEASELOCK", "MARK", "UNMARK", "USEPROXY", "PROXY"], // Lolicode-specific
        namespaceFollows: ["namespace", "using"],
        parenFollows: ["if", "for", "while", "switch", "foreach", "using", "catch", "when"],
        operators: ["=", "??", "||", "&&", "|", "^", "&", "==", "!=", "<=", ">=", "<<", "+", "-", "*", "/", "%", "!", "~", "++", "--", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", ">>", "=>"],
        symbols: /[=><!~?:&|+\-*\/\^%]+/,
        escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,
        includeLF: true,
        tokenizer: {
            root: [
                [/^[ \t]*\#\#.*\n/, "comment"],
                [/^[ \t]*\!.*\n/, "disabled"],
                [/\#[^ ]+/, "block.label"],
                [/FUNCTION/, "block.function"],
                [/KEYCHECK/, "block.keycheck"],
                [/KEYCHAIN/, "block.keycheck.keychain"],
                [/(N?M?R?KEY|VALUE)/, "block.keycheck.keychain.key"],
                [/(RECAPTCHA|SOLVECAPTCHA)/, "block.recaptcha"],
                [/REQUEST/, "block.request"],
                [/HEADER/, "block.request.header"],
                [/COOKIE/, "block.request.cookie"],
                [/PARSE/, "block.parse"],
                [/(LR|CSS|JSON|REGEX)/, "block.parse.mode"],
                [/(CAPTCHA|REPORTCAPTCHA)/, "block.captcha"],
                [/TCP/, "block.tcp"],
                [/UTILITY/, "block.utility"],
                [/NAVIGATE/, "block.navigate"],
                [/BROWSERACTION/, "block.browseraction"],
                [/ELEMENTACTION/, "block.elementaction"],
                [/EXECUTEJS/, "block.executejs"],
                [/(IF|ELSE|ENDIF|WHILE|ENDWHILE|JUMP|BEGIN|END|SCRIPT)/, "keyword"],
                [/(PRINT|SET|DELETE)/, "command"],
                [/G?VAR/, "variable"],
                [/CAP/, "capture"],
                [/->/, "arrow"],

                [/\"/, "string", "string"],
                [/[0-9\.]+]/, "number"]
            ],
            string: [
                [/[^\\"<]+/, "string"],
                [/@escapes/, "string.escape"],
                [/<[^"]+>/, "variable"],
                [/\\./, "string.escape.invalid"],
                [/"/, {
                    token: "string.quote",
                    next: "@pop"
                }]
            ],
        }
    });
}
