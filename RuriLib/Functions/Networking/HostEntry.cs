namespace RuriLib.Functions.Networking
{
    public struct HostEntry
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public HostEntry(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}
