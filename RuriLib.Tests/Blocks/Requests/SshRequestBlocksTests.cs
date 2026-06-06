using RuriLib.Exceptions;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils.Mockup;
using System.Threading.Tasks;
using SshMethods = RuriLib.Blocks.Requests.Ssh.Methods;
using Xunit;
using BotProviders = RuriLib.Models.Bots.Providers;

namespace RuriLib.Tests.Blocks.Requests;

public class SshRequestBlocksTests
{
    [Fact]
    public async Task SshRunCommand_WithoutClient_Throws()
    {
        var data = NewBotData();

        var ex = await Assert.ThrowsAsync<BlockExecutionException>(() =>
            SshMethods.SshRunCommandAsync(data, "ls"));

        Assert.Equal("The SSH client is not initialized", ex.Message);
    }

    private static BotData NewBotData()
        => new(
            new BotProviders(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("hello", new WordlistType()));
}
