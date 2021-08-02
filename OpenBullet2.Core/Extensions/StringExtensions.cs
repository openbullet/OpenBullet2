using System.Globalization;
using System.Text;

namespace OpenBullet2.Core.Extensions
{
    public static class StringExtensions
    {
        public static string BeautifyName(this string name)
        {
            StringBuilder sb = new();

            foreach (var c in name)
            {
                // Replace anything, but letters and digits, with space
                if (!char.IsLetterOrDigit(c))
                {
                    sb.Append(' ');
                }
                else
                {
                    sb.Append(c);
                }
            }

            return CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(sb.ToString().ToLower());
        }
    }
}
