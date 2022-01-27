using System;
using System.Collections;
using System.Net;
using System.Reflection;

namespace RuriLib.Functions.Http
{
    public static class Http
    {
        public static CookieCollection GetAllCookies(CookieContainer cookieJar)
        {
            var cookieCollection = new CookieCollection();

            var table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable",
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                null, cookieJar, Array.Empty<object>());

            foreach (var tableKey in table.Keys)
            {
                var str_tableKey = (string)tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey[1..];
                }

                var list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                    BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance,
                    null, table[tableKey], Array.Empty<object>());

                foreach (var listKey in list.Keys)
                {
                    var url = "https://" + str_tableKey + (string)listKey;
                    cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
                }
            }

            return cookieCollection;
        }
    }
}
