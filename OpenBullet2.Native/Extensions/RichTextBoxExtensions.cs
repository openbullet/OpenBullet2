using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace OpenBullet2.Native.Extensions
{
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            var tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd)
            {
                Text = text
            };
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        public static string[] Lines(this RichTextBox box)
        {
            var textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            return textRange.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetText(this RichTextBox box)
            => new TextRange(box.Document.ContentStart, box.Document.ContentEnd).Text;

        public static string GetTextFromLines(this RichTextBox box)
            => box.Lines().Aggregate((current, next) => current + next);

        public static string Select(this RichTextBox rtb, int offset, int length, Color color)
        {
            // Get text selection:
            var textRange = rtb.Selection;

            // Get text starting point:
            var start = rtb.Document.ContentStart;

            // Get begin and end requested:
            var startPos = GetTextPointAt(start, offset);
            var endPos = GetTextPointAt(start, offset + length);

            // New selection of text:
            textRange.Select(startPos, endPos);

            // Apply property to the selection:
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, brush);

            // Return selection text:
            return rtb.Selection.Text;
        }

        public static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            var ret = from;
            var i = 0;

            while ((i < pos) && (ret != null))
            {
                if (ret.GetPointerContext(LogicalDirection.Backward) is TextPointerContext.Text or TextPointerContext.None)
                {
                    i++;
                }

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                {
                    return ret;
                }

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }
    }
}
