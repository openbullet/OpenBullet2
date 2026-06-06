using RuriLib.Functions.Networking;
using RuriLib.Providers.Emails;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Providers.Emails;

[Collection(nameof(CurrentDirectoryCollection))]
public class FileEmailDomainRepositoryTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetServers_MissingDomain_ReturnsEmptySequence() => await WithCurrentDirectoryAsync(async () =>
                                                                              {
                                                                                  var repository = new FileEmailDomainRepository();

                                                                                  Assert.Empty(await repository.GetImapServers("example.com"));
                                                                                  Assert.Empty(await repository.GetPop3Servers("example.com"));
                                                                                  Assert.Empty(await repository.GetSmtpServers("example.com"));
                                                                              });

    [Fact]
    public async Task TryAddServer_NewDomain_PersistsAndReturnsEntry() => await WithCurrentDirectoryAsync(async () =>
                                                                               {
                                                                                   var repository = new FileEmailDomainRepository();
                                                                                   var entry = new HostEntry("imap.example.com", 993);

                                                                                   await repository.TryAddImapServer("example.com", entry);

                                                                                   var servers = (await repository.GetImapServers("example.com")).ToList();
                                                                                   Assert.Single(servers);
                                                                                   Assert.Equal("imap.example.com", servers[0].Host);
                                                                                   Assert.Equal(993, servers[0].Port);
                                                                                   Assert.Contains($"example.com:{entry.Host}:{entry.Port}", await File.ReadAllTextAsync(Path.Combine("UserData", "imapdomains.dat"), TestCancellationToken));
                                                                               });

    private static async Task WithCurrentDirectoryAsync(Func<Task> action)
    {
        var originalDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(FileEmailDomainRepositoryTests)}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            Directory.SetCurrentDirectory(tempDirectory);
            await action();
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(tempDirectory, true);
        }
    }
}

[CollectionDefinition(nameof(CurrentDirectoryCollection), DisableParallelization = true)]
public class CurrentDirectoryCollection;
