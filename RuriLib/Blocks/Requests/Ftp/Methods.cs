using FluentFTP;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentFTP.Proxy.AsyncProxy;
using RuriLib.Models.Proxies;

namespace RuriLib.Blocks.Requests.Ftp
{
    [BlockCategory("FTP", "Blocks to work with the FTP protocol", "#fbec5d")]
    public static class Methods
    {
        [Block("Connects to an FTP server")]
        public static async Task FtpConnect(BotData data, string host, int port = 21, 
            string username = "", string password = "", int timeoutMilliseconds = 10000)
        {
            data.Logger.LogHeader();

            AsyncFtpClient client;
            
            var ftpConfig = new FtpConfig
            {
                ConnectTimeout = timeoutMilliseconds,
                DataConnectionConnectTimeout = timeoutMilliseconds,
                DataConnectionReadTimeout = timeoutMilliseconds,
                ReadTimeout = timeoutMilliseconds
            };

            if (data.UseProxy && data.Proxy is not null)
            {
                var proxyInfo = new FtpProxyProfile
                {
                    ProxyHost = data.Proxy.Host,
                    ProxyPort = data.Proxy.Port,
                    ProxyCredentials = new NetworkCredential(data.Proxy.Username, data.Proxy.Password)
                };

                client = data.Proxy.Type switch 
                {
                    ProxyType.Http => new AsyncFtpClientHttp11Proxy(proxyInfo),
                    ProxyType.Socks4 => new AsyncFtpClientSocks4Proxy(proxyInfo),
                    ProxyType.Socks4a => new AsyncFtpClientSocks4aProxy(proxyInfo),
                    ProxyType.Socks5 => new AsyncFtpClientSocks5Proxy(proxyInfo),
                    _ => throw new Exception($"Unsupported proxy type: {data.Proxy.Type}")
                };
            }
            else
            {
                client = new AsyncFtpClient();
            }
            
            client.Host = host;
            client.Port = port;
            client.Credentials = new NetworkCredential(username, password);
            client.Config = ftpConfig;

            data.SetObject("ftpClient", client);
            client.LegacyLogger = InitLogger(data);
            await client.AutoConnect(data.CancellationToken).ConfigureAwait(false);
            
            if (!client.IsConnected)
            {
                throw new Exception("Failed to connect to the FTP server with the given credentials");
            }

            data.Logger.Log($"Connected to {host}:{port}", LogColors.Maize);
        }

        [Block("Lists the folders on the FTP server")]
        public static async Task<List<string>> FtpListItems(BotData data, FtpItemKind kind = FtpItemKind.FilesAndFolders, bool recursive = false)
        {
            data.Logger.LogHeader();
            var client = GetClient(data);
            var options = recursive ? FtpListOption.Recursive : FtpListOption.Auto;
            var list = new List<string>();

            foreach (var item in await client.GetListing("/", options).ConfigureAwait(false))
            {
                switch (item.Type)
                {
                    case FtpObjectType.Directory when kind is FtpItemKind.FilesAndFolders or FtpItemKind.Folder:
                        data.Logger.Log(item.FullName, LogColors.Maize);
                        list.Add(item.FullName);
                        break;
                    
                    case FtpObjectType.File when kind is FtpItemKind.FilesAndFolders or FtpItemKind.File:
                        data.Logger.Log(item.FullName, LogColors.Maize);
                        list.Add(item.FullName);
                        break;
                }
            }

            return list;
        }

        [Block("Downloads a file from the FTP server")]
        public static async Task FtpDownloadFile(BotData data, string remoteFileName, string localFileName)
        {
            data.Logger.LogHeader();
            var client = GetClient(data);
            await client.DownloadFile(localFileName, remoteFileName, FtpLocalExists.Overwrite, 
                FtpVerify.None, null, data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"{remoteFileName} downloaded to {localFileName}", LogColors.Maize);
        }

        [Block("Downloads a folder from the FTP server")]
        public static async Task FtpDownloadFolder(BotData data, string remoteDir, string localDir, 
            FtpLocalExists existsPolicy = FtpLocalExists.Skip)
        {
            data.Logger.LogHeader();
            var client = GetClient(data);
            await client.DownloadDirectory(localDir, remoteDir, FtpFolderSyncMode.Update, existsPolicy,
                FtpVerify.None, null, null, data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"{remoteDir} downloaded to {localDir}", LogColors.Maize);
        }

        [Block("Uploads a file to the FTP server")]
        public static async Task FtpUploadFile(BotData data, string remoteFileName, string localFileName,
            FtpRemoteExists existsPolicy = FtpRemoteExists.Overwrite)
        {
            data.Logger.LogHeader();
            var client = GetClient(data);
            await client.UploadFile(localFileName, remoteFileName, existsPolicy, true, 
                FtpVerify.None, null, data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log($"{localFileName} uploaded to {remoteFileName}", LogColors.Maize);
        }

        [Block("Disconnects from the connected FTP server")]
        public static async Task FtpDisconnect(BotData data)
        {
            data.Logger.LogHeader();
            var client = GetClient(data);
            await client.Disconnect(data.CancellationToken).ConfigureAwait(false);

            data.Logger.Log("Disconnected from the FTP server", LogColors.Maize);
        }

        [Block("Gets the protocol log", name = "Get FTP Log")]
        public static string FtpGetLog(BotData data)
        {
            data.Logger.LogHeader();

            var protocolLogger = data.TryGetObject<StringBuilder>("ftpLogger");
            var log = protocolLogger.ToString();

            data.Logger.Log(log, LogColors.Maize);

            return log;
        }

        private static Action<FtpTraceLevel, string> InitLogger(BotData data)
        {
            var protocolLogger = new StringBuilder();
            data.SetObject("ftpLogger", protocolLogger);

            return new Action<FtpTraceLevel, string>((traceLevel, message)
                => protocolLogger.AppendLine($"[{traceLevel}] {message}"));
        }

        private static AsyncFtpClient GetClient(BotData data)
            => data.TryGetObject<AsyncFtpClient>("ftpClient") ?? throw new Exception("Connect to a server first!");
    }

    public enum FtpItemKind
    {
        Folder,
        File,
        FilesAndFolders
    }
}
