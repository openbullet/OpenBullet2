namespace RuriLib.Proxies.Helpers;

internal static class PortHelper
{
    public static bool ValidateTcpPort(int port)
        => port is >= 1 and <= 65535;
}
