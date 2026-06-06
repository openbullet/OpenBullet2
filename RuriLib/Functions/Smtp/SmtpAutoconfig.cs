using System;
using RuriLib.Functions.Networking;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace RuriLib.Functions.Smtp;

/// <summary>
/// Parses SMTP Thunderbird-style autoconfig XML.
/// </summary>
public static class SmtpAutoconfig
{
    /// <summary>
    /// Parses SMTP server entries from autoconfig XML.
    /// </summary>
    /// <param name="xml">The autoconfig XML.</param>
    /// <returns>The parsed SMTP host entries.</returns>
    public static List<HostEntry> Parse(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);

        var servers = doc.DocumentElement?.SelectNodes(
            "/clientConfig/emailProvider/outgoingServer[contains(@type,'smtp')]");

        var hosts = new List<HostEntry>();

        if (servers is null)
        {
            return hosts;
        }

        foreach (XmlNode server in servers)
        {
            var hostname = GetRequiredNodeValue(server, "hostname");
            var portValue = GetRequiredNodeValue(server, "port");

            if (!int.TryParse(portValue, NumberStyles.None, CultureInfo.InvariantCulture, out var port))
            {
                throw new FormatException($"Invalid SMTP port '{portValue}' in autoconfig XML.");
            }

            hosts.Add(new HostEntry(hostname, port));
        }

        return hosts;
    }

    private static string GetRequiredNodeValue(XmlNode server, string nodeName)
        => server.SelectSingleNode(nodeName)?.InnerText
            ?? throw new FormatException($"Missing {nodeName} in SMTP autoconfig XML.");
}
