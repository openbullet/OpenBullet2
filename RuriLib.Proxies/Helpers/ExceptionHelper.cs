namespace RuriLib.Proxies.Helpers
{
    static internal class ExceptionHelper
    {
        public static bool ValidateTcpPort(int port)
            => port >= 1 && port <= 65535;
    }
}
