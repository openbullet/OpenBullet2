function registerLoliCode() {
	// Register a new language
	monaco.languages.register({ id: 'lolicode' });

	// Register a tokens provider for the language
	monaco.languages.setMonarchTokensProvider('lolicode', {

        keywords: ["extern", "alias", "using", "bool", "decimal", "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "char", "float", "double", "object", "dynamic", "string", "assembly", "is", "as", "ref", "out", "this", "base", "new", "typeof", "void", "checked", "unchecked", "default", "delegate", "var", "const", "if", "else", "switch", "case", "while", "do", "for", "foreach", "in", "break", "continue", "goto", "return", "throw", "try", "catch", "finally", "lock", "yield", "from", "let", "where", "join", "on", "equals", "into", "orderby", "ascending", "descending", "select", "group", "by", "namespace", "partial", "class", "field", "event", "method", "param", "property", "public", "protected", "internal", "private", "abstract", "sealed", "static", "struct", "readonly", "volatile", "virtual", "override", "params", "get", "set", "add", "remove", "operator", "true", "false", "implicit", "explicit", "interface", "enum", "null", "async", "await", "fixed", "sizeof", "stackalloc", "unsafe", "nameof", "when",
            "JUMP", "REPEAT", "END", "FOREACH", "IN", "LOG", "CLOG", "WHILE", "IF", "ELSE", "ELSE IF", "TRY", "CATCH", "LOCK", "SET", "TAKE", "TAKEONE", "FROM", "FINALLY", "ACQUIRELOCK", "RELEASELOCK"], // Lolicode-specific
        namespaceFollows: ["namespace", "using"],
        parenFollows: ["if", "for", "while", "switch", "foreach", "using", "catch", "when"],
        operators: ["=", "??", "||", "&&", "|", "^", "&", "==", "!=", "<=", ">=", "<<", "+", "-", "*", "/", "%", "!", "~", "++", "--", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", ">>", "=>"],
        symbols: /[=><!~?:&|+\-*\/\^%]+/,
        escapes: /\\(?:[abfnrtv\\"']|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})/,
        includeLF: true,
        tokenizer: {
            root: [
                // Jump to lolicode section
                [/^[ \t]*BLOCK:.*/, "block", "@lolicode"],

                // Needed to not mess up syntax highlighting in some cases. There are still cases that are not handled.
                [/^[ \t]*(JUMP|REPEAT|END|FOREACH|LOG|CLOG|WHILE|IF|ELSE|ELSE IF|TRY|CATCH|LOCK|SET|TAKE|TAKEONE|FINALLY|ACQUIRELOCK|RELEASELOCK) ?/, "block", "@consumeline"],

                [/\@?[a-zA-Z_]\w*/, {
                    cases: {
                        "@namespaceFollows": {
                            token: "keyword.$0",
                            next: "@namespace"
                        },
                        "@keywords": {
                            token: "keyword.$0",
                            next: "@qualified"
                        },
                        "@default": {
                            token: "identifier",
                            next: "@qualified"
                        }
                    }
                }], {
                    include: "@whitespace"
                },
                [/}/, {
                    cases: {
                        "$S2==interpolatedstring": {
                            token: "string.quote",
                            next: "@pop"
                        },
                        "$S2==litinterpstring": {
                            token: "string.quote",
                            next: "@pop"
                        },
                        "@default": "@brackets"
                    }
                }],
                [/[{}()\[\]]/, "@brackets"],
                [/[<>](?!@symbols)/, "@brackets"],
                [/@symbols/, {
                    cases: {
                        "@operators": "delimiter",
                        "@default": ""
                    }
                }],
                [/[0-9_]*\.[0-9_]+([eE][\-+]?\d+)?[fFdD]?/, "number.float"],
                [/0[xX][0-9a-fA-F_]+/, "number.hex"],
                [/0[bB][01_]+/, "number.hex"],
                [/[0-9_]+/, "number"],
                [/[;,.]/, "delimiter"],
                [/"([^"\\]|\\.)*$/, "string.invalid"],
                [/"/, {
                    token: "string.quote",
                    next: "@string"
                }],
                [/\$\@"/, {
                    token: "string.quote",
                    next: "@litinterpstring"
                }],
                [/\@"/, {
                    token: "string.quote",
                    next: "@litstring"
                }],
                [/\$"/, {
                    token: "string.quote",
                    next: "@interpolatedstring"
                }],
                [/'[^\\']'/, "string"],
                [/(')(@escapes)(')/, ["string", "string.escape", "string"]],
                [/'/, "string.invalid"]
            ],
            qualified: [
                [/[a-zA-Z_][\w]*/, {
                    cases: {
                        "@keywords": {
                            token: "keyword.$0"
                        },
                        "@default": "identifier"
                    }
                }],
                [/\./, "delimiter"],
                ["", "", "@pop"]
            ],
            namespace: [{
                include: "@whitespace"
            },
            [/[A-Z]\w*/, "namespace"],
            [/[\.=]/, "delimiter"],
            ["", "", "@pop"]
            ],
            comment: [
                [/[^\/*]+/, "comment"],
                ["\\*/", "comment", "@pop"],
                [/[\/*]/, "comment"]
            ],
            string: [
                [/[^\\"<]+/, "string"], // Added '<' for Lolicode
                [/@escapes/, "string.escape"],
                [/<[^>]+>/, "string.variable"], // Lolicode specific
                [/\\./, "string.escape.invalid"],
                [/"/, {
                    token: "string.quote",
                    next: "@pop"
                }]
            ],
            litstring: [
                [/[^"<]+/, "string"], // Added '<' for Lolicode
                [/""/, "string.escape"],
                [/<[^>]+>/, "string.variable"], // Lolicode specific
                [/"/, {
                    token: "string.quote",
                    next: "@pop"
                }]
            ],
            litinterpstring: [
                [/[^"{<]+/, "string"], // Added '<' for Lolicode
                [/""/, "string.escape"],
                [/{{/, "string.escape"],
                [/}}/, "string.escape"],
                [/<[^>]+>/, "string.variable"], // Lolicode specific
                [/{/, {
                    token: "string.quote",
                    next: "root.litinterpstring"
                }],
                [/"/, {
                    token: "string.quote",
                    next: "@pop"
                }]
            ],
            interpolatedstring: [
                [/[^\\"{<]+/, "string"], // Added '<' for Lolicode
                [/@escapes/, "string.escape"],
                [/\\./, "string.escape.invalid"],
                [/{{/, "string.escape"],
                [/}}/, "string.escape"],
                [/<[^>]+>/, "string.variable"], // Lolicode specific
                [/{/, {
                    token: "string.quote",
                    next: "root.interpolatedstring"
                }],
                [/"/, {
                    token: "string.quote",
                    next: "@pop"
                }]
            ],
            whitespace: [
                [/^[ \t\v\f]*#((r)|(load))(?=\s)/, "directive.csx"],
                [/^[ \t\v\f]*#\w.*$/, "namespace.cpp"],
                [/[ \t\v\f\r\n]+/, ""],
                [/\/\*/, "comment", "@comment"],
                [/\/\/.*$/, "comment"]
            ],
            lolicode: [
                [/^[ \t]*ENDBLOCK\n$/, "block.end", "@pop"],
                [/^[ \t]*DISABLED\n$/, "block.disabled"],
                [/^[ \t]*SAFE\n$/, "block.safe"],
                [/^[ \t]*LABEL:.*/, "block.label"],
                [/\bVAR\b/, "block.var"],
                [/\bCAP\b/, "block.cap"],
                [/\$/, "block.interp"],
                [/\@[A-Za-z0-9_\.]+/, "block.variable"],
                [/\b=\>\b/, "block.arrow"],
                [/"/, {
                    token: "string.quote",
                    next: "@string"
                }],
                [/[0-9_]*\.[0-9_]+([eE][\-+]?\d+)?[fFdD]?/, "number.float"],
                [/0[xX][0-9a-fA-F_]+/, "number.hex"],
                [/0[bB][01_]+/, "number.hex"],
                [/[0-9_]+/, "number"]
            ],
            consumeline: [
                [/\n/, "none", "@pop"]
            ]
        }
	});

    /*
	monaco.languages.registerCompletionItemProvider('lolicode', {
		provideCompletionItems: () => {
			var suggestions = [{
				label: 'simpleText',
				kind: monaco.languages.CompletionItemKind.Text,
				insertText: 'simpleText'
			}, {
				label: 'testing',
				kind: monaco.languages.CompletionItemKind.Keyword,
				insertText: 'testing(${1:condition})',
				insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet
			}, {
				label: 'ifelse',
				kind: monaco.languages.CompletionItemKind.Snippet,
				insertText: [
					'if (${1:condition}) {',
					'\t$0',
					'} else {',
					'\t',
					'}'
				].join('\n'),
				insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
				documentation: 'If-Else Statement'
			}];
			return { suggestions: suggestions };
		}
	});
    */
}
