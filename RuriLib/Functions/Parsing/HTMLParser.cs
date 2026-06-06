using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Functions.Parsing;

/// <summary>
/// Parses HTML documents with CSS selectors and XPath queries.
/// </summary>
public static class HtmlParser
{
    /// <summary>
    /// Parses the value of an attribute from all elements that match a given selector in an HTML page.
    /// </summary>
    /// <param name="htmlPage">The HTML page</param>
    /// <param name="cssSelector">The CSS Selector that targets the desired elements</param>
    /// <param name="attributeName">The attribute for which you want to parse the value</param>
    /// <returns>The parsed attribute values.</returns>
    public static IEnumerable<string> QueryAttributeAll(string htmlPage, string cssSelector, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(htmlPage);
        ArgumentNullException.ThrowIfNull(cssSelector);
        ArgumentNullException.ThrowIfNull(attributeName);

        var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(htmlPage);
        var elements = document.QuerySelectorAll(cssSelector);

        return attributeName switch
        {
            "innerText" => elements.Select(e => e.TextContent),
            "innerHTML" => elements.Select(e => e.InnerHtml),
            "outerHTML" => elements.Select(e => e.OuterHtml),
            _ => elements.Select(e => e.GetAttribute(attributeName)).OfType<string>(),
        };
    }

    /// <summary>
    /// Parses the value of an attribute from all elements that match a given xpath in an HTML page.
    /// </summary>
    /// <param name="htmlPage">The HTML page</param>
    /// <param name="xPath">The XPath that targets the desired elements</param>
    /// <param name="attributeName">The attribute for which you want to parse the value</param>
    /// <returns>The parsed attribute values.</returns>
    public static IEnumerable<string> QueryXPathAll(string htmlPage, string xPath, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(htmlPage);
        ArgumentNullException.ThrowIfNull(xPath);
        ArgumentNullException.ThrowIfNull(attributeName);

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlPage);
        var elements = htmlDoc.DocumentNode.SelectNodes(xPath);

        if (elements is null)
        {
            return Enumerable.Empty<string>();
        }

        return attributeName switch
        {
            "innerText" => elements.Select(e => e.InnerText),
            "innerHTML" => elements.Select(e => e.InnerHtml),
            "outerHTML" => elements.Select(e => e.OuterHtml),
            _ => elements.Select(e => e.Attributes[attributeName]?.Value).OfType<string>(),
        };
    }
}
