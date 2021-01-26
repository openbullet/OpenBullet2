using BlazorMonaco;
using BlazorMonaco.Bridge;
using OpenBullet2.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class MonacoThemeSetter
    {
		public static async Task SetLolicodeTheme(PersistentSettingsService Settings)
        {
			await MonacoEditorBase.DefineTheme("vs-loli", new StandaloneThemeData
			{
				Base = Settings.OpenBulletSettings.CustomizationSettings.MonacoTheme,
				Inherit = true,
				Rules = new List<TokenThemeRule>
				{
					new TokenThemeRule { Token = "block", Foreground = "5DADE2" },
					new TokenThemeRule { Token = "block.end", Foreground = "5DADE2" },
					new TokenThemeRule { Token = "block.disabled", Foreground = "EC7063" },
					new TokenThemeRule { Token = "block.label", Foreground = "58D68D" },
					new TokenThemeRule { Token = "block.var", Foreground = "F7DC6F" },
					new TokenThemeRule { Token = "block.cap", Foreground = "F1948A" },
					new TokenThemeRule { Token = "block.arrow", Foreground = "BB8FCE" },
					new TokenThemeRule { Token = "block.interp", Foreground = "BB8FCE" },
					new TokenThemeRule { Token = "block.variable", Foreground = "FAD7A0" },
					new TokenThemeRule { Token = "string.variable", Foreground = "FAD7A0" }
				}
			});

			await MonacoEditorBase.SetTheme("vs-loli");
		}
    }
}
