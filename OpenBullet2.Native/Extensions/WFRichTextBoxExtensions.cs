using System;
using System.Windows.Forms;

namespace OpenBullet2.Native.Extensions
{
    public static class WFRichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, string color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = System.Drawing.ColorTranslator.FromHtml(color);
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
            box.AppendText(Environment.NewLine);
        }

        /// <summary>
        /// This NEEDS to be called each time we write a log message on high refresh rate boxes to prevent memory leaks.
        /// The stupid documentation is wrong, it says it only clears the most recent operation but it actually clears the entire buffer.
        /// https://stackoverflow.com/questions/14455452/system-windows-forms-richtextboxs-implementation-of-textboxbase-clearundo
        /// </summary>
        public static void ClearUndoHistory(this RichTextBox box) => box.ClearUndo();
    }
}
