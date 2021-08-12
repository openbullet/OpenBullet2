using System.Text;

namespace OpenBullet2.Native.Utils
{
    public class HtmlStyler
    {
        private readonly string html;
        private readonly StringBuilder sb;

        public HtmlStyler(string html)
        {
            this.html = html;
            sb = new StringBuilder();
            sb.Append("<html><head><style> body { ");
        }

        public HtmlStyler WithStyle(string name, string value)
        {
            sb.Append($"{name}: {value}; ");
            return this;
        }

        public override string ToString()
        {
            // Finalize the style
            sb.Append($"}} </style><body>{html}</body></html>");
            return sb.ToString();
        }
    }
}
