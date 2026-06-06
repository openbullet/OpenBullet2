using System;
using System.Collections;
using System.Net;
using System.Reflection;

namespace RuriLib.Functions.Http;

/// <summary>
/// Provides HTTP-related utility methods.
/// </summary>
public static class Http
{
    /// <summary>
    /// Gets all cookies stored in a <see cref="CookieContainer"/>.
    /// </summary>
    /// <param name="cookieJar">The cookie container to inspect.</param>
    /// <returns>A flattened collection with all cookies from all domains and paths.</returns>
    public static CookieCollection GetAllCookies(CookieContainer cookieJar)
    {
        var cookieCollection = new CookieCollection();
        var table = GetDomainTable(cookieJar);

        foreach (DictionaryEntry domainEntry in table)
        {
            if (domainEntry.Key is not string domainKey || domainKey.Length == 0)
            {
                continue;
            }

            var normalizedDomain = domainKey[0] == '.'
                ? domainKey[1..]
                : domainKey;

            var list = GetPathList(domainEntry.Value);

            foreach (DictionaryEntry pathEntry in list)
            {
                if (pathEntry.Key is not string path)
                {
                    continue;
                }

                cookieCollection.Add(cookieJar.GetCookies(new Uri($"https://{normalizedDomain}{path}")));
            }
        }

        return cookieCollection;
    }

    private static Hashtable GetDomainTable(CookieContainer cookieJar)
        => cookieJar.GetType().InvokeMember("m_domainTable",
            BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
            null, cookieJar, []) as Hashtable
            ?? throw new InvalidOperationException("Could not access the cookie domain table.");

    private static SortedList GetPathList(object? domainEntry)
        => domainEntry?.GetType().InvokeMember("m_list",
            BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
            null, domainEntry, []) as SortedList
            ?? throw new InvalidOperationException("Could not access the cookie path list.");
}
