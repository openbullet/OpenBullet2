using Renci.SshNet;
using RuriLib.Attributes;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Proxies;
using System;
using RuriLib.Exceptions;

namespace RuriLib.Blocks.Requests.Ssh;

[BlockCategory("SSH", "Blocks for SSH", "#6699ff")]
public static class Methods
{
    /// <summary>
    /// Logs in via SSH with the given credentials.
    /// </summary>
    [Block("Logs in via SSH with the given credentials", name = "Authenticate (Password)")]
    public static void SshAuthenticateWithPassword(BotData data, string host, int port = 22, string username = "root",
        string password = "", int timeoutMilliseconds = 30000, int channelTimeoutMilliseconds = 1000, int retryAttempts = 10)
    {
        data.Logger.LogHeader();

        ConnectionInfo info;

        if (data.UseProxy && data.Proxy is not null)
        {
            info = new ConnectionInfo(host, port, username, TranslateProxyType(data.Proxy.Type), data.Proxy.Host,
                data.Proxy.Port, data.Proxy.Username, data.Proxy.Password,
                new PasswordAuthenticationMethod(username, password));
        }
        else
        {
            info = new ConnectionInfo(host, port, username,
                new PasswordAuthenticationMethod(username, password));
        }

        info.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
        info.ChannelCloseTimeout = TimeSpan.FromMilliseconds(channelTimeoutMilliseconds);
        info.RetryAttempts = retryAttempts;
            
        var client = new SshClient(info);
        client.Connect();

        data.SetObject("sshClient", client);

        data.Logger.Log($"Connected to {host} on port {port} as {username}", "#526ab4");
    }

    /// <summary>
    /// Logs in via SSH with no credentials.
    /// </summary>
    [Block("Logs in via SSH with no credentials", name = "Authenticate (None)")]
    public static void SshAuthenticateWithNone(BotData data, string host, int port = 22, string username = "root",
        int timeoutMilliseconds = 30000, int channelTimeoutMilliseconds = 1000, int retryAttempts = 10)
    {
        data.Logger.LogHeader();

        ConnectionInfo info;

        if (data.UseProxy && data.Proxy is not null)
        {
            info = new ConnectionInfo(host, port, username, TranslateProxyType(data.Proxy.Type), data.Proxy.Host,
                data.Proxy.Port, data.Proxy.Username, data.Proxy.Password,
                new NoneAuthenticationMethod(username));
        }
        else
        {
            info = new ConnectionInfo(host, port, username,
                new NoneAuthenticationMethod(username));
        }

        info.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
        info.ChannelCloseTimeout = TimeSpan.FromMilliseconds(channelTimeoutMilliseconds);
        info.RetryAttempts = retryAttempts;

        var client = new SshClient(info);
        client.Connect();

        data.SetObject("sshClient", client);

        data.Logger.Log($"Connected to {host} on port {port} as {username}", "#526ab4");
    }

    /// <summary>
    /// Logs in via SSH with a private key stored in the given file.
    /// </summary>
    [Block("Logs in via SSH with a private key stored in the given file", name = "Authenticate (Private Key)")]
    public static void SshAuthenticateWithPK(BotData data, string host, int port = 22, string username = "root",
        string keyFile = "rsa.key", string keyFilePassword = "", int timeoutMilliseconds = 30000,
        int channelTimeoutMilliseconds = 1000, int retryAttempts = 10)
    {
        data.Logger.LogHeader();

        ConnectionInfo info;
        var pk = new PrivateKeyFile(keyFile, keyFilePassword);
        IPrivateKeySource[] keyFiles = [pk];

        if (data is { UseProxy: true, Proxy: not null })
        {
            info = new ConnectionInfo(host, port, username, TranslateProxyType(data.Proxy.Type), data.Proxy.Host,
                data.Proxy.Port, data.Proxy.Username, data.Proxy.Password,
                new PrivateKeyAuthenticationMethod(username, keyFiles));
        }
        else
        {
            info = new ConnectionInfo(host, port, username,
                new PrivateKeyAuthenticationMethod(username, keyFiles));
        }

        info.Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
        info.ChannelCloseTimeout = TimeSpan.FromMilliseconds(channelTimeoutMilliseconds);
        info.RetryAttempts = retryAttempts;

        var client = new SshClient(info);
        client.Connect();

        data.SetObject("sshClient", client);

        data.Logger.Log($"Connected to {host} on port {port} as {username}", "#526ab4");
    }

    /// <summary>
    /// Executes a command via SSH", name = "Run Command.
    /// </summary>
    [Block("Executes a command via SSH", name = "Run Command")]
    public static string SshRunCommand(BotData data, string command)
    {
        data.Logger.LogHeader();

        var client = data.TryGetObject<SshClient>("sshClient");
        
        if (client is null)
        {
            throw new BlockExecutionException("The SSH client is not initialized");
        }

        data.Logger.Log($"> {command}", "#526ab4");
        var cmd = client.RunCommand(command);

        if (string.IsNullOrWhiteSpace(cmd.Error))
        {
            data.Logger.Log(cmd.Result, "#526ab4");
            return cmd.Result;
        }

        data.Logger.Log(cmd.Error, LogColors.RedOrange);
        return cmd.Error;
    }

    private static ProxyTypes TranslateProxyType(ProxyType type)
        => type switch
        {
            ProxyType.Http => ProxyTypes.Http,
            ProxyType.Socks4 => ProxyTypes.Socks4,
            ProxyType.Socks5 => ProxyTypes.Socks5,
            _ => throw new NotImplementedException()
        };
}
