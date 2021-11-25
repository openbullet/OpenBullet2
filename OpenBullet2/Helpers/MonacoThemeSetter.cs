using BlazorMonaco;
using OpenBullet2.Core.Models.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class MonacoThemeSetter
    {
		public static async Task SetLoliCodeTheme(CustomizationSettings settings)
        {
			await MonacoEditorBase.DefineTheme("vs-lolicode", new StandaloneThemeData
			{
				Base = settings.MonacoTheme,
				Inherit = true,
				Rules = new List<TokenThemeRule>
				{
					new TokenThemeRule { Token = "block", Foreground = "98CFFF" },
					new TokenThemeRule { Token = "block.end", Foreground = "98CFFF" },
					new TokenThemeRule { Token = "block.disabled", Foreground = "EC7063" },
					new TokenThemeRule { Token = "block.safe", Foreground = "BCFF70" },
					new TokenThemeRule { Token = "block.label", Foreground = "58D68D" },
					new TokenThemeRule { Token = "block.var", Foreground = "F7DC6F" },
					new TokenThemeRule { Token = "block.cap", Foreground = "F1948A" },
					new TokenThemeRule { Token = "block.arrow", Foreground = "BB8FCE" },
					new TokenThemeRule { Token = "block.interp", Foreground = "BB8FCE" },
					new TokenThemeRule { Token = "block.variable", Foreground = "FAD7A0" },
                    new TokenThemeRule { Token = "block.customparam", Foreground = "A8FFD2" },
                    new TokenThemeRule { Token = "string.variable", Foreground = "FAD7A0" },                    
					new TokenThemeRule { Token = "jumplabel", Foreground = "F78686" },
					new TokenThemeRule { Token = "key", Foreground = "6F81FF" },
					new TokenThemeRule { Token = "false", Foreground = "FF6347" },
					new TokenThemeRule { Token = "true", Foreground = "9ACD32" },
					new TokenThemeRule { Token = "keychain.success", Foreground = "9ACD32" },
					new TokenThemeRule { Token = "keychain.fail", Foreground = "FF6347" },
					new TokenThemeRule { Token = "keychain.retry", Foreground = "FFFF00" },
					new TokenThemeRule { Token = "keychain.ban", Foreground = "DDA0DD" },
					new TokenThemeRule { Token = "keychain.none", Foreground = "87CEEB" },
					new TokenThemeRule { Token = "keychain.default", Foreground = "FFA500" },
					new TokenThemeRule { Token = "script.delimiter", Foreground = "6F81FF" },
					new TokenThemeRule { Token = "script.output", Foreground = "FFBC80" },
				}
			});

			await MonacoEditorBase.SetTheme("vs-lolicode");
		}

        public static async Task SetLoliScriptTheme(CustomizationSettings settings)
        {
            await MonacoEditorBase.DefineTheme("vs-loliscript", new StandaloneThemeData
            {
                Base = settings.MonacoTheme,
                Inherit = true,
                Rules = new List<TokenThemeRule>
                {
                    new TokenThemeRule { Token = "comment", Foreground = "808080" },
                    new TokenThemeRule { Token = "disabled", Foreground = "FF6347" },
                    new TokenThemeRule { Token = "variable", Foreground = "FFFF00" },
                    new TokenThemeRule { Token = "capture", Foreground = "FF6347" },
                    new TokenThemeRule { Token = "arrow", Foreground = "FF00FF" },
                    new TokenThemeRule { Token = "number", Foreground = "8FBC8B" },
                    new TokenThemeRule { Token = "string", Foreground = "ADD8E6" },
                    new TokenThemeRule { Token = "block.label", Foreground = "FFDAB9" },
                    new TokenThemeRule { Token = "block.function", Foreground = "ADFF2F" },
                    new TokenThemeRule { Token = "block.keycheck", Foreground = "1E90FF" },
                    new TokenThemeRule { Token = "block.keycheck.keychain", Foreground = "DDA0DD" },
                    new TokenThemeRule { Token = "block.keycheck.keychain.key", Foreground = "87CEEB" },
                    new TokenThemeRule { Token = "block.recaptcha", Foreground = "40E0D0" },
                    new TokenThemeRule { Token = "block.request", Foreground = "32CD32" },
                    new TokenThemeRule { Token = "block.request.header", Foreground = "DDA0DD" },
                    new TokenThemeRule { Token = "block.request.cookie", Foreground = "87CEEB" },
                    new TokenThemeRule { Token = "block.parse", Foreground = "FFD700" },
                    new TokenThemeRule { Token = "block.parse.mode", Foreground = "FFA500" },
                    new TokenThemeRule { Token = "block.captcha", Foreground = "FF8C00" },
                    new TokenThemeRule { Token = "block.tcp", Foreground = "9370DB" },
                    new TokenThemeRule { Token = "block.utility", Foreground = "F5DEB3" },
                    new TokenThemeRule { Token = "block.navigate", Foreground = "4169E1" },
                    new TokenThemeRule { Token = "block.browseraction", Foreground = "008000" },
                    new TokenThemeRule { Token = "block.elementaction", Foreground = "B22222" },
                    new TokenThemeRule { Token = "block.executejs", Foreground = "4B0082" },
                    new TokenThemeRule { Token = "keyword", Foreground = "FF4500" },
                    new TokenThemeRule { Token = "command", Foreground = "FFA500" },
                }
            });

            await MonacoEditorBase.SetTheme("vs-loliscript");
        }
    }
}
