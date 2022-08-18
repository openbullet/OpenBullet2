using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Proxy;
using MailKit.Search;
using RuriLib.Attributes;
using RuriLib.Functions.Http;
using RuriLib.Functions.Imap;
using RuriLib.Functions.Networking;
using RuriLib.Http.Models;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RuriLib.Functions.Time.TimeConverter;

namespace RuriLib.Blocks.Requests.Imap
{
    [BlockCategory("IMAP", "Blocks for working with the IMAP protocol", "#93c", "#fff")]
    public static class Methods
    {
        private static readonly List<string> subdomains = new() { "mail", "imap-mail", "inbound", "in", "mx", "imap", "imaps", "m" };
        
        [Block("Connects to an IMAP server by automatically detecting the host and port")]
        public static async Task ImapAutoConnect(BotData data, string email, int timeoutMilliseconds = 60000)
        {
            data.Logger.LogHeader();

            var protocolLogger = InitLogger(data);

            var client = new ImapClient(protocolLogger)
            {
                Timeout = timeoutMilliseconds,
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };

            if (data.UseProxy && data.Proxy != null)
            {
                client.ProxyClient = MapProxyClient(data);
            }

            data.SetObject("imapClient", client);

            var domain = email.Split('@')[1];

            // Try the entries from imapdomains.dat
            var candidates = (await data.Providers.EmailDomains.GetImapServers(domain).ConfigureAwait(false)).ToList();

            foreach (var c in candidates)
            {
                var success = await TryConnect(data, client, domain, c).ConfigureAwait(false);

                if (success)
                {
                    return;
                }
            }

            // Thunderbird autoconfig
            candidates.Clear();
            var thunderbirdUrl = $"{"https"}://live.mozillamessaging.com/autoconfig/v1.1/{domain}";
            try
            {
                var xml = await GetString(data, thunderbirdUrl).ConfigureAwait(false);
                candidates = ImapAutoconfig.Parse(xml);
                data.Logger.Log($"Queried {thunderbirdUrl} and got {candidates.Count} server(s)", LogColors.DarkOrchid);
            }
            catch
            {
                data.Logger.Log($"Failed to query {thunderbirdUrl}", LogColors.DarkOrchid);
            }

            foreach (var c in candidates)
            {
                var success = await TryConnect(data, client, domain, c).ConfigureAwait(false);

                if (success)
                {
                    return;
                }
            }

            // Site autoconfig
            candidates.Clear();
            var autoconfigUrl = $"https://autoconfig.{domain}/mail/config-v1.1.xml?emailaddress={email}";
            var autoconfigUrlUnsecure = $"http://autoconfig.{domain}/mail/config-v1.1.xml?emailaddress={email}";
            try
            {
                string xml;

                try
                {
                    xml = await GetString(data, autoconfigUrl).ConfigureAwait(false);
                }
                catch
                {
                    xml = await GetString(data, autoconfigUrlUnsecure).ConfigureAwait(false);
                }

                candidates = ImapAutoconfig.Parse(xml);
                data.Logger.Log($"Queried {autoconfigUrl} and got {candidates.Count} server(s)", LogColors.DarkOrchid);
            }
            catch
            {
                data.Logger.Log($"Failed to query {autoconfigUrl} (both https and http)", LogColors.DarkOrchid);
            }

            foreach (var c in candidates)
            {
                var success = await TryConnect(data, client, domain, c).ConfigureAwait(false);

                if (success)
                {
                    return;
                }
            }

            // Site well-known
            candidates.Clear();
            var wellKnownUrl = $"https://{domain}/.well-known/autoconfig/mail/config-v1.1.xml";
            var wellKnownUrlUnsecure = $"http://{domain}/.well-known/autoconfig/mail/config-v1.1.xml";
            try
            {
                string xml;

                try
                {
                    xml = await GetString(data, wellKnownUrl).ConfigureAwait(false);
                }
                catch
                {
                    xml = await GetString(data, wellKnownUrlUnsecure).ConfigureAwait(false);
                }

                candidates = ImapAutoconfig.Parse(xml);
                data.Logger.Log($"Queried {wellKnownUrl} and got {candidates.Count} server(s)", LogColors.DarkOrchid);
            }
            catch
            {
                data.Logger.Log($"Failed to query {wellKnownUrl} (both https and http)", LogColors.DarkOrchid);
            }

            foreach (var c in candidates)
            {
                var success = await TryConnect(data, client, domain, c).ConfigureAwait(false);

                if (success)
                {
                    return;
                }
            }

            // Try the domain itself and possible subdomains
            candidates.Clear();
            candidates.Add(new HostEntry(domain, 993));
            candidates.Add(new HostEntry(domain, 143));

            foreach (var sub in subdomains)
            {
                candidates.Add(new HostEntry($"{sub}.{domain}", 993));
                candidates.Add(new HostEntry($"{sub}.{domain}", 143));
            }

            foreach (var c in candidates)
            {
                var success = await TryConnect(data, client, domain, c).ConfigureAwait(false);

                if (success)
                {
                    return;
                }
            }

            // Try MX records
            candidates.Clear();
            try
            {
                var mxRecords = await DnsLookup.FromGoogle(domain, "MX", data.Proxy, 30000, data.CancellationToken).ConfigureAwait(false);
                mxRecords.ForEach(r =>
                {
                    candidates.Add(new HostEntry(r, 993));
                    candidates.Add(new HostEntry(r, 143));
                });

                data.Logger.Log($"Queried the MX records and got {candidates.Count} server(s)", LogColors.DarkOrchid);
            }
            catch
            {
                data.Logger.Log($"Failed to query the MX records", LogColors.DarkOrchid);
            }

            foreach (var c in candidates)
            {
                var success = await TryConnect(data, client, domain, c).ConfigureAwait(false);

                if (success)
                {
                    return;
                }
            }

            throw new Exception("Exhausted all possibilities, failed to connect!");
        }

        private static async Task<bool> TryConnect(BotData data, ImapClient client, string domain, HostEntry entry)
        {
            data.Logger.Log($"Trying {entry.Host} on port {entry.Port}...", LogColors.DarkOrchid);

            try
            {
                await client.ConnectAsync(entry.Host, entry.Port, MailKit.Security.SecureSocketOptions.Auto, data.CancellationToken).ConfigureAwait(false);
                data.Logger.Log($"Connected! SSL/TLS: {client.IsSecure}", LogColors.DarkOrchid);
                await data.Providers.EmailDomains.TryAddImapServer(domain, entry).ConfigureAwait(false);
                return true;
            }
            catch
            {
                data.Logger.Log($"Failed!", LogColors.DarkOrchid);
            }

            return false;
        }

        private static async Task<string> GetString(BotData data, string url)
        {
            using var httpClient = HttpFactory.GetRLHttpClient(data.Proxy, new()
            {
                ConnectTimeout = TimeSpan.FromMilliseconds(30000),
                ReadWriteTimeout = TimeSpan.FromMilliseconds(30000)
            });

            using var request = new HttpRequest
            {
                Uri = new Uri(url),
            };

            using var response = await httpClient.SendAsync(request, data.CancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync(data.CancellationToken).ConfigureAwait(false);
        }

        [Block("Connects to an IMAP server")]
        public static async Task ImapConnect(BotData data, string host, int port, int timeoutMilliseconds = 60000)
        {
            data.Logger.LogHeader();

            var protocolLogger = InitLogger(data);

            var client = new ImapClient(protocolLogger)
            {
                Timeout = timeoutMilliseconds,
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };

            if (data.UseProxy && data.Proxy != null)
            {
                client.ProxyClient = MapProxyClient(data);
            }

            data.SetObject("imapClient", client);

            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.Auto, data.CancellationToken).ConfigureAwait(false);
            data.Logger.Log($"Connected to {host} on port {port}. SSL/TLS: {client.IsSecure}", LogColors.DarkOrchid);
        }

        [Block("Disconnects from an IMAP server")]
        public static async Task ImapDisconnect(BotData data)
        {
            data.Logger.LogHeader();

            var client = GetClient(data);

            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, data.CancellationToken).ConfigureAwait(false);
                data.Logger.Log($"Client disconnected", LogColors.DarkOrchid);
            }
            else
            {
                data.Logger.Log($"The client was not connected", LogColors.DarkOrchid);
            }
        }

        [Block("Logs into an account")]
        public static async Task ImapLogin(BotData data, string email, string password, bool openInbox = true, int timeoutMilliseconds = 10000)
        {
            data.Logger.LogHeader();

            var client = GetClient(data);
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            using var cts = new CancellationTokenSource(timeoutMilliseconds);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, data.CancellationToken);
            await client.AuthenticateAsync(email, password, linkedCts.Token).ConfigureAwait(false);
            data.Logger.Log("Authenticated successfully", LogColors.DarkOrchid);

            if (openInbox)
            {
                await client.Inbox.OpenAsync(FolderAccess.ReadWrite, data.CancellationToken).ConfigureAwait(false);
                data.Logger.Log($"Opened the inbox, there are {client.Inbox.Count} total messages", LogColors.DarkOrchid);
            }
        }

        [Block("Gets the protocol log", name = "Get Imap Log")]
        public static string ImapGetLog(BotData data)
        {
            data.Logger.LogHeader();

            var protocolLogger = data.TryGetObject<ProtocolLogger>("imapLogger");
            var bytes = (protocolLogger.Stream as MemoryStream).ToArray();
            var log = Encoding.UTF8.GetString(bytes);

            data.Logger.Log(log, LogColors.DarkOrchid);

            return log;
        }

        [Block("Opens the inbox folder")]
        public static async Task ImapOpenInbox(BotData data)
        {
            data.Logger.LogHeader();

            var client = GetAuthenticatedClient(data);
            await client.Inbox.OpenAsync(FolderAccess.ReadWrite, data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"Opened the inbox, there are {client.Inbox.Count} total messages", LogColors.DarkOrchid);
        }

        [Block("Searches for mails", extraInfo = "The 'delivered after' expects a Unix timestamp (UTC) in seconds.")]
        public static async Task<List<string>> ImapSearchMails(BotData data, SearchField field1 = SearchField.Subject, string text1 = "",
            SearchField field2 = SearchField.From, string text2 = "", int deliveredAfter = 1)
        {
            data.Logger.LogHeader();

            var inbox = GetOpenInbox(data);

            SearchQuery query = new DateSearchQuery(SearchTerm.DeliveredAfter, ((long)deliveredAfter).ToDateTimeUtc());

            if (!string.IsNullOrEmpty(text1))
            {
                query = query.And(new TextSearchQuery(MapSearchTerm(field1), text1));
            }

            if (!string.IsNullOrEmpty(text2))
            {
                query = query.And(new TextSearchQuery(MapSearchTerm(field2), text2));
            }

            IList<UniqueId> mails = null;

            try
            {
                mails = await inbox.SearchAsync(query, data.CancellationToken).ConfigureAwait(false);
            }
            catch
            {
                data.Logger.Log("Search denied by the server", LogColors.DarkOrchid);
                return new();
            }

            var ids = mails.Select(id => id.Id.ToString()).ToList();

            data.Logger.Log($"{ids.Count} mails matched the search", LogColors.DarkOrchid);
            data.Logger.Log(ids, LogColors.DarkOrchid);

            return ids;
        }

        [Block("Gets a text (or HTML) representation of a mail")]
        public static async Task<string> ImapReadMail(BotData data, string id, bool preferHtml = false)
        {
            data.Logger.LogHeader();

            var inbox = GetOpenInbox(data);
            var uniqueId = new UniqueId(uint.Parse(id));
            var mail = await inbox.GetMessageAsync(uniqueId, data.CancellationToken).ConfigureAwait(false);
            var body = mail.TextBody;

            if (string.IsNullOrEmpty(body) || preferHtml)
            {
                body = mail.HtmlBody;
            }

            var output =
$@"From: {mail.From.First()}
To: {mail.To.First()}
Subject: {mail.Subject}
Body:
{body}";

            data.Logger.Log($"From: {mail.From.First()}", LogColors.DarkOrchid);
            data.Logger.Log($"To: {mail.To.First()}", LogColors.DarkOrchid);
            data.Logger.Log($"Subject: {mail.Subject}", LogColors.DarkOrchid);
            data.Logger.Log("Body:", LogColors.DarkOrchid);
            data.Logger.Log(body, LogColors.DarkOrchid, true);
            return output;
        }

        [Block("Gets a mail in EML format")]
        public static async Task<byte[]> ImapReadMailRaw(BotData data, string id)
        {
            data.Logger.LogHeader();

            var inbox = GetOpenInbox(data);
            var uniqueId = new UniqueId(uint.Parse(id));
            var mail = await inbox.GetMessageAsync(uniqueId, data.CancellationToken).ConfigureAwait(false);

            using var ms = new MemoryStream();
            await mail.WriteToAsync(ms, data.CancellationToken);
            ms.Seek(0, SeekOrigin.Begin);
            var bytes = ms.ToArray();

            data.Logger.Log($"Received {bytes.Length} bytes", LogColors.DarkOrchid);

            return bytes;
        }

        [Block("Deletes a mail", name = "Delete Mail")]
        public static async Task ImapDeleteMail(BotData data, string id)
        {
            data.Logger.LogHeader();

            var inbox = GetOpenInbox(data);
            var uniqueId = new UniqueId(uint.Parse(id));
            await inbox.AddFlagsAsync(uniqueId, MessageFlags.Deleted, true, data.CancellationToken).ConfigureAwait(false);
            await inbox.ExpungeAsync(data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"Deleted mail with id {id}", LogColors.DarkOrchid);
        }

        private static ImapClient GetClient(BotData data)
            => data.TryGetObject<ImapClient>("imapClient") ?? throw new Exception("Connect the IMAP client first!");

        private static ImapClient GetAuthenticatedClient(BotData data)
        {
            var client = GetClient(data);

            if (!client.IsAuthenticated)
            {
                throw new Exception("Authenticate the IMAP client first!");
            }

            return client;
        }

        private static IMailFolder GetOpenInbox(BotData data)
        {
            var inbox = GetClient(data).Inbox;

            if (!inbox.IsOpen)
            {
                throw new Exception("Open the inbox first!");
            }

            return inbox;
        }

        private static IProxyClient MapProxyClient(BotData data)
        {
            if (data.Proxy.NeedsAuthentication)
            {
                var creds = new NetworkCredential(data.Proxy.Username, data.Proxy.Password);

                return data.Proxy.Type switch
                {
                    Models.Proxies.ProxyType.Http => new HttpProxyClient(data.Proxy.Host, data.Proxy.Port, creds),
                    Models.Proxies.ProxyType.Socks4 => new Socks4Client(data.Proxy.Host, data.Proxy.Port, creds),
                    Models.Proxies.ProxyType.Socks4a => new Socks4aClient(data.Proxy.Host, data.Proxy.Port, creds),
                    Models.Proxies.ProxyType.Socks5 => new Socks5Client(data.Proxy.Host, data.Proxy.Port, creds),
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                return data.Proxy.Type switch
                {
                    Models.Proxies.ProxyType.Http => new HttpProxyClient(data.Proxy.Host, data.Proxy.Port),
                    Models.Proxies.ProxyType.Socks4 => new Socks4Client(data.Proxy.Host, data.Proxy.Port),
                    Models.Proxies.ProxyType.Socks4a => new Socks4aClient(data.Proxy.Host, data.Proxy.Port),
                    Models.Proxies.ProxyType.Socks5 => new Socks5Client(data.Proxy.Host, data.Proxy.Port),
                    _ => throw new NotImplementedException(),
                };
            }
        }

        private static SearchTerm MapSearchTerm(SearchField field) => field switch
        {
            SearchField.To => SearchTerm.ToContains,
            SearchField.From => SearchTerm.FromContains,
            SearchField.Subject => SearchTerm.SubjectContains,
            SearchField.Body => SearchTerm.BodyContains,
            _ => throw new NotImplementedException()
        };

        private static ProtocolLogger InitLogger(BotData data)
        {
            var ms = new MemoryStream();
            var protocolLogger = new ProtocolLogger(ms, true);
            data.SetObject("imapLoggerStream", ms);
            data.SetObject("imapLogger", protocolLogger);

            return protocolLogger;
        }
    }
}
