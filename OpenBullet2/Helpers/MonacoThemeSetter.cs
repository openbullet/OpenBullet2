using BlazorMonaco;
using OpenBullet2.Core.Models.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class MonacoThemeSetter
    {
		public static async Task SetLolicodeTheme(CustomizationSettings settings)
        {
			await MonacoEditorBase.DefineTheme("vs-loli", new StandaloneThemeData
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

			await MonacoEditorBase.SetTheme("vs-loli");
		}
    }
}
