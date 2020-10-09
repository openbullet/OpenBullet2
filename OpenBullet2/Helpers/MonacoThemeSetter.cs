using BlazorMonaco;
using BlazorMonaco.Bridge;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Helpers
{
    public static class MonacoThemeSetter
    {
		public static async Task SetLolicodeTheme()
        {
			await MonacoEditorBase.DefineTheme("vs-loli", new StandaloneThemeData
			{
				Base = "vs-dark",
				Inherit = true,
				Rules = new List<TokenThemeRule>
				{
					new TokenThemeRule { Token = "block-delimiter", Foreground = "5DADE2" },
					new TokenThemeRule { Token = "disabled", Foreground = "EC7063" },
					new TokenThemeRule { Token = "label", Foreground = "58D68D" },
					new TokenThemeRule { Token = "variable", Foreground = "F7DC6F" },
					new TokenThemeRule { Token = "capture", Foreground = "F1948A" },
					new TokenThemeRule { Token = "arrow", Foreground = "BB8FCE" },
					new TokenThemeRule { Token = "interpolated-symbol", Foreground = "BB8FCE" },
					new TokenThemeRule { Token = "variable-symbol", Foreground = "FAD7A0" }
				}
			});

			await MonacoEditorBase.SetTheme("vs-loli");
		}
    }
}
