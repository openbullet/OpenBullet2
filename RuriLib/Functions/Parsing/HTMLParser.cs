using HtmlAgilityPack;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Functions.Parsing
{
    public static class HtmlParser
    {
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

            var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlPage);
            var elements = document.QuerySelectorAll(cssSelector);

            return attributeName switch
            {
                "innerText" => elements.Select(e => e.TextContent),
                "innerHTML" => elements.Select(e => e.InnerHtml),
                "outerHTML" => elements.Select(e => e.OuterHtml),
                _ => elements
                    .Select(e => e.Attributes.FirstOrDefault(a => a.Name == attributeName))
                    .Where(a => a != null)
                    .Select(a => a.Value),
            };
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
                return new List<string>();

            return attributeName switch
            {
                "innerText" => elements.Select(e => e.InnerText),
                "innerHTML" => elements.Select(e => e.InnerHtml),
                "outerHTML" => elements.Select(e => e.OuterHtml),
                _ => elements
                    .Select(e => e.Attributes.FirstOrDefault(a => a.Name == attributeName))
                    .Where(a => a != null)
                    .Select(a => a.Value),
            };
        }
    }
}
