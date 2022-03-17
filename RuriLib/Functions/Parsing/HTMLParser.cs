using AngleSharp.Dom;
using HtmlAgilityPack;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Functions.Parsing
{
    public static class HtmlParser
    {
        private static readonly Dictionary<string, Func<IHtmlCollection<IElement>, IEnumerable<string>>> _getCssAttributesFunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            { "innerText", GetCssTextContentAttributes },
            { "innerHTML", GetCssInnerHtmlAttributes },
            { "outerHTML", GetCssOuterHtmlAttributes }
        };
        private static readonly Dictionary<string, Func<HtmlNodeCollection, IEnumerable<string>>> _getXPathAttributesFunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            { "innerText", GetXPathTextContentAttributes },
            { "innerHTML", GetXPathInnerHtmlAttributes },
            { "outerHTML", GetXPathOuterHtmlAttributes }
        };

        /// <summary>
        /// Parses the value of an attribute from all elements that match a given selector in an HTML page.
        /// </summary>
        /// <param name="htmlPage">The HTML page</param>
        /// <param name="cssSelector">The CSS Selector that targets the desired elements</param>
        /// <param name="attributeName">The attribute for which you want to parse the value</param>
        public static IEnumerable<string> QueryAttributeAll(string htmlPage, string cssSelector, string attributeName)
        {
            if (htmlPage == null)
                throw new ArgumentNullException(nameof(htmlPage));

            if (cssSelector == null)
                throw new ArgumentNullException(nameof(cssSelector));

            if (attributeName == null)
                throw new ArgumentNullException(nameof(attributeName));

            using var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlPage);
            var elements = document.QuerySelectorAll(cssSelector);

            return _getCssAttributesFunctions.ContainsKey(attributeName) ? _getCssAttributesFunctions[attributeName].Invoke(elements) : elements.Where(e => e.HasAttribute(attributeName)).Select(e => e.GetAttribute(attributeName));
        }

        /// <summary>
        /// Parses the value of an attribute from all elements that match a given xpath in an HTML page.
        /// </summary>
        /// <param name="htmlPage">The HTML page</param>
        /// <param name="xPath">The XPath that targets the desired elements</param>
        /// <param name="attributeName">The attribute for which you want to parse the value</param>
        public static IEnumerable<string> QueryXPathAll(string htmlPage, string xPath, string attributeName)
        {
            if (htmlPage == null)
                throw new ArgumentNullException(nameof(htmlPage));

            if (xPath == null)
                throw new ArgumentNullException(nameof(xPath));

            if (attributeName == null)
                throw new ArgumentNullException(nameof(attributeName));

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlPage);
            var elements = htmlDoc.DocumentNode.SelectNodes(xPath);

            if (elements == null)
                return Array.Empty<string>();

            return _getXPathAttributesFunctions.ContainsKey(attributeName) ? _getXPathAttributesFunctions[attributeName].Invoke(elements) : elements.Select(e => e.GetAttributeValue(attributeName, string.Empty));
        }

        private static IEnumerable<string> GetCssTextContentAttributes(IHtmlCollection<IElement> elements) => elements.Select(e => e.TextContent);

        private static IEnumerable<string> GetCssInnerHtmlAttributes(IHtmlCollection<IElement> elements) => elements.Select(e => e.InnerHtml);

        private static IEnumerable<string> GetCssOuterHtmlAttributes(IHtmlCollection<IElement> elements) => elements.Select(e => e.OuterHtml);

        private static IEnumerable<string> GetXPathTextContentAttributes(HtmlNodeCollection htmlNodes) => htmlNodes.Select(e => e.InnerText);

        private static IEnumerable<string> GetXPathInnerHtmlAttributes(HtmlNodeCollection elements) => elements.Select(e => e.InnerHtml);

        private static IEnumerable<string> GetXPathOuterHtmlAttributes(HtmlNodeCollection elements) => elements.Select(e => e.OuterHtml);
    }
}
