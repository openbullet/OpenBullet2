using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using Svg;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RuriLib.Blocks.Utility.Images
{
    [BlockCategory("Images", "Blocks for working with images", "#fad6a5")]
    public static class Methods
    {
        [Block("Converts an svg image to a byte array containing a png image")]
        public static byte[] SvgToPng(BotData data, string xml, int width = 300, int height = 150)
        {
            data.Logger.LogHeader();

            var doc = SvgDocument.FromSvg<SvgDocument>(xml);
            using var ms = new MemoryStream();
            using var bitmap = new Bitmap(width, height);
            doc.Draw(bitmap);
            bitmap.Save(ms, ImageFormat.Png);

            data.Logger.Log("Converted the svg to png", LogColors.Flavescent);

            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
    }
}
