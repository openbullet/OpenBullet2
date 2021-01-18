namespace RuriLib.Providers.Proxies
{
    public interface IGlobalProxyKeysProvider
    {
        bool ContainsBanKey(string text, bool caseSensitive = false);
        bool ContainsRetryKey(string text, bool caseSensitive = false);
    }
}
