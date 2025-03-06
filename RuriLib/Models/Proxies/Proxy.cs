using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Proxies;

/// <summary>
/// A proxy.
/// </summary>
public class Proxy
{
    /// <summary>
    /// The unique identifier of the proxy.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The host of the proxy.
    /// </summary>
    public string Host { get; set; }
    
    /// <summary>
    /// The port of the proxy.
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// The username for authentication (if needed).
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// The password for authentication (if needed).
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// The type of the proxy.
    /// </summary>
    public ProxyType Type { get; set; }

    /// <summary>
    /// The working status of the proxy.
    /// </summary>
    public ProxyWorkingStatus WorkingStatus { get; set; } = ProxyWorkingStatus.Untested;
    
    /// <summary>
    /// The country of the proxy (Unknown by default).
    /// </summary>
    public string Country { get; set; } = "Unknown";
    
    /// <summary>
    /// The ping of the proxy in milliseconds.
    /// </summary>
    public int Ping { get; set; } = 0;

    /// <summary>
    /// The last time the proxy was used.
    /// </summary>
    public DateTime? LastUsed { get; set; }
    
    /// <summary>
    /// The last time the proxy was checked.
    /// </summary>
    public DateTime? LastChecked { get; set; }
    
    /// <summary>
    /// The last time the proxy was banned.
    /// </summary>
    public DateTime? LastBanned { get; set; }

    /// <summary>
    /// The total number of times the proxy was used.
    /// </summary>
    public int TotalUses { get; set; } = 0;
    
    /// <summary>
    /// How many bots are currently using the proxy.
    /// </summary>
    public int BeingUsedBy { get; set; } = 0;
    
    /// <summary>
    /// The status of the proxy.
    /// </summary>
    public ProxyStatus ProxyStatus { get; set; } = ProxyStatus.Available;

    /// <summary>
    /// Whether the proxy needs authentication.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public bool NeedsAuthentication => !string.IsNullOrWhiteSpace(Username);
        
    /// <summary>
    /// The protocol of the proxy.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [JsonIgnore]
    public string Protocol => Type.ToString().ToLower();

    /// <summary>
    /// Creates a new instance of the Proxy class.
    /// </summary>
    public Proxy(string host, int port, ProxyType type = ProxyType.Http, string username = "", string password = "")
    {
        // TODO: Username and password should be nullable and default to null
        
        Host = host;
        Port = port;
        Type = type;
        Username = username;
        Password = password;
    }

    /// <summary>
    /// Parses a Proxy from a string. See examples for accepted inputs.
    /// </summary>
    /// <example>Proxy.Parse("127.0.0.1:8000")</example>
    /// <example>Proxy.Parse("127.0.0.1:8000:username:password")</example>
    /// <example>Proxy.Parse("(socks5)127.0.0.1:8000")</example>
    public static bool TryParse(string proxyString, out Proxy? proxy, ProxyType defaultType = ProxyType.Http,
        string defaultUsername = "", string defaultPassword = "")
    {
        try
        {
            proxy = Parse(proxyString, defaultType, defaultUsername, defaultPassword);
            return true;
        }
        catch
        {
            proxy = null;
            return false;
        }
    }

    /// <summary>
    /// Parses a Proxy from a string. See examples for accepted inputs.
    /// </summary>
    /// <example>Proxy.Parse("127.0.0.1:8000")</example>
    /// <example>Proxy.Parse("127.0.0.1:8000:username:password")</example>
    /// <example>Proxy.Parse("(socks5)127.0.0.1:8000")</example>
    public static Proxy Parse(string proxyString, ProxyType defaultType = ProxyType.Http,
        string defaultUsername = "", string defaultPassword = "")
    {
        ArgumentNullException.ThrowIfNull(proxyString);
        ArgumentNullException.ThrowIfNull(defaultUsername);
        ArgumentNullException.ThrowIfNull(defaultPassword);

        var proxy = new Proxy(string.Empty, 0, defaultType, defaultUsername, defaultPassword);

        // If the type was specified, parse it and remove it from the string
        if (proxyString.StartsWith('('))
        {
            var groups = Regex.Match(proxyString, @"^\((.*)\)").Groups;

            if (Enum.TryParse<ProxyType>(groups[1].Value, true, out var type))
            {
                proxy.Type = type;
            }
            else
            {
                throw new FormatException("Invalid proxy type");
            }

            proxyString = Regex.Replace(proxyString, @"^\((.*)\)", "");
        }

        if (!proxyString.Contains(':'))
        {
            throw new FormatException("Expected at least 2 colon-separated fields");
        }

        var fields = proxyString.Split(':');
        proxy.Host = fields[0];

        if (int.TryParse(fields[1], out var port))
        {
            proxy.Port = port;
        }
        else
        {
            throw new FormatException("The proxy port must be an integer");
        }

        switch (fields.Length)
        {
            case 3:
                throw new FormatException("Expected 4 colon-separated fields, got 3");
            // Set the other two if they are present
            case > 2:
                proxy.Username = fields[2];
                proxy.Password = fields[3];
                break;
        }

        return proxy;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // TODO: Make these properties get-only
        return Host.GetHashCode() + Port.GetHashCode() + Type.GetHashCode()
               + Username.GetHashCode() + Password.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Type != ProxyType.Http)
        {
            sb
                .Append('(')
                .Append(Type)
                .Append(')');
        }

        sb
            .Append(Host)
            .Append(':')
            .Append(Port);

        if (!string.IsNullOrWhiteSpace(Username))
        {
            sb
                .Append(':')
                .Append(Username)
                .Append(':')
                .Append(Password);
        }

        return sb.ToString();
    }
}
