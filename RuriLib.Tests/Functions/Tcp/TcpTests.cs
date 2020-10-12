using RuriLib.Functions.Tcp;
using System;
using System.Net.Security;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Functions.Tcp
{
    public class TcpTests
    {
        private readonly string imapHost = "mail.openbullet.dev";
        private readonly int imapPort = 143;

        private readonly string imapSslHost = "server248.web-hosting.com";
        private readonly int imapSslPort = 993;

        //private readonly string httpHost = "example.com";
        //private readonly int httpPort = 80;

        //private readonly string httpsHost = "example.com";
        //private readonly int httpsPort = 443;

        [Fact]
        public void ConnectAndSend_ImapNoProxy_ConnectAndLogin()
        {
            var netStream = TcpFactory.GetNetworkStream(imapHost, imapPort, TimeSpan.FromSeconds(5), null);

            string response;
            byte[] buffer = new byte[2048];
            var bytes = netStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("OK", response);

            var message = "01 LOGIN hello@hello.com password\r\n";            
            var toSend = Encoding.ASCII.GetBytes(message);
            netStream.Write(toSend, 0, toSend.Length);

            bytes = netStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("NO", response);

            netStream.Close();
        }

        /*
        [Fact]
        public void ConnectAndSend_ImapWithProxy_ConnectAndLogin()
        {
            var proxy = Proxy.Parse("(socks5)PUT_PROXY_HERE");
            var netStream = TcpFactory.GetNetworkStream(imapHost, imapPort, TimeSpan.FromSeconds(5), proxy);

            string response;
            byte[] buffer = new byte[2048];
            var bytes = netStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("OK", response);

            var message = "01 LOGIN hello@hello.com password\r\n";
            var toSend = Encoding.ASCII.GetBytes(message);
            netStream.Write(toSend, 0, toSend.Length);

            bytes = netStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("NO", response);

            netStream.Close();
        }

        [Fact]
        public void ConnectAndSend_HttpWithProxy_GetPage()
        {
            var proxy = Proxy.Parse("(http)PUT_PROXY_HERE");
            var netStream = TcpFactory.GetNetworkStream(httpHost, httpPort, TimeSpan.FromSeconds(5), proxy);

            var message = $"GET / HTTP/1.1\r\nHost: {httpHost}\r\n\r\n";
            var toSend = Encoding.ASCII.GetBytes(message);
            netStream.Write(toSend, 0, toSend.Length);

            byte[] buffer = new byte[2048];
            var bytes = netStream.Read(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("Example Domain", response);

            netStream.Close();
        }
        */

        [Fact]
        public void ConnectAndSend_ImapSSLNoProxy_ConnectAndLogin()
        {
            var netStream = TcpFactory.GetNetworkStream(imapSslHost, imapSslPort, TimeSpan.FromSeconds(5), null);

            var sslStream = new SslStream(netStream);
            sslStream.AuthenticateAsClient(imapSslHost);

            string response;
            byte[] buffer = new byte[2048];
            var bytes = sslStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("OK", response);

            var message = "01 LOGIN hello@hello.com password\r\n";
            var toSend = Encoding.ASCII.GetBytes(message);
            sslStream.Write(toSend, 0, toSend.Length);

            bytes = sslStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("NO", response);

            sslStream.Close();
            netStream.Close();
        }

        /*
        [Fact]
        public void ConnectAndSend_ImapSSLWithProxy_ConnectAndLogin()
        {
            var proxy = Proxy.Parse("(socks5)PUT_PROXY_HERE");
            var netStream = TcpFactory.GetNetworkStream(imapSslHost, imapSslPort, TimeSpan.FromSeconds(5), proxy);

            var sslStream = new SslStream(netStream);
            sslStream.AuthenticateAsClient(imapSslHost);

            string response;
            byte[] buffer = new byte[2048];
            var bytes = sslStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("OK", response);

            var message = "01 LOGIN hello@hello.com password\r\n";
            var toSend = Encoding.ASCII.GetBytes(message);
            sslStream.Write(toSend, 0, toSend.Length);

            bytes = sslStream.Read(buffer, 0, buffer.Length);
            response = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("NO", response);

            sslStream.Close();
            netStream.Close();
        }

        [Fact]
        public void ConnectAndSend_HttpsWithProxy_GetPage()
        {
            var proxy = Proxy.Parse("(http)PUT_PROXY_HERE");
            var netStream = TcpFactory.GetNetworkStream(httpsHost, httpsPort, TimeSpan.FromSeconds(5), proxy);

            var sslStream = new SslStream(netStream);
            sslStream.AuthenticateAsClient(httpsHost);

            var message = $"GET / HTTP/1.1\r\nHost: {httpsHost}\r\n\r\n";
            var toSend = Encoding.ASCII.GetBytes(message);
            sslStream.Write(toSend, 0, toSend.Length);

            byte[] buffer = new byte[2048];
            
            // Read headers
            var bytes = sslStream.Read(buffer, 0, buffer.Length);
            var headers = Encoding.ASCII.GetString(buffer, 0, bytes);

            Thread.Sleep(100);

            // Read payload
            bytes = sslStream.Read(buffer, 0, buffer.Length);
            var payload = Encoding.ASCII.GetString(buffer, 0, bytes);

            Assert.Contains("Example Domain", payload);

            sslStream.Close();
            netStream.Close();
        }
        */
    }
}
