using System;

namespace OpenBullet2.Native.Extensions
{
    public static class WFRichTextBoxExtensions
    {
        public static void AppendText(this System.Windows.Forms.RichTextBox box, string text, string color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = System.Drawing.ColorTranslator.FromHtml(color);
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.AppendText(Environment.NewLine);
        }
    }
}
