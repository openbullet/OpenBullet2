using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace RuriLib.Functions.Parsing
{
    public static class XmlParser
    {
        /// <summary>
        /// Parses the value of an attribute from all elements that match a given xpath in an XML page.
        /// </summary>
        /// <param name="xmlPage">The XML page</param>
        /// <param name="xPath">The XPath that targets the desired elements</param>
        /// <param name="attributeName">The attribute for which you want to parse the value</param>
        public static IEnumerable<string> QueryXPathAll(string xmlPage, string xPath, string attributeName)
        {
            if (xmlPage == null)
                throw new ArgumentNullException(nameof(xmlPage));

            if (xPath == null)
                throw new ArgumentNullException(nameof(xPath));

            if (attributeName == null)
                throw new ArgumentNullException(nameof(attributeName));

            var document = new XmlDocument();
            document.LoadXml(xmlPage);
            var elements = document.SelectNodes(xPath) as IList<XmlNode>;

            return attributeName switch
            {
                "innerText" => elements.Select(e => e.InnerText),
                "innerXml" => elements.Select(e => e.InnerXml),
                "outerXml" => elements.Select(e => e.OuterXml),
                _ => elements
                    .Select(e => e.Attributes.Cast<XmlAttribute>().FirstOrDefault(a => a.Name == attributeName))
                    .Where(a => a != null)
                    .Select(a => a.Value),
            };
        }
    }
}
