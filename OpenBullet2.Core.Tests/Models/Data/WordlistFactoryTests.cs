using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using RuriLib.Exceptions;
using RuriLib.Services;

namespace OpenBullet2.Core.Tests.Models.Data;

public sealed class WordlistFactoryTests : IDisposable
{
    private readonly string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    [Fact]
    public void FromEntity_ValidEntity_MapsWordlistMetadata()
    {
        var settings = new RuriLibSettingsService(tempDir);
        var factory = new WordlistFactory(settings);
        var entity = new WordlistEntity
        {
            Id = 123,
            Name = "Credentials List",
            FileName = Path.Combine(tempDir, "creds.txt"),
            Type = "Credentials",
            Purpose = "Login",
            Total = 3_000_000_000L
        };

        var wordlist = factory.FromEntity(entity);

        Assert.Equal(123, wordlist.Id);
        Assert.Equal("Credentials List", wordlist.Name);
        Assert.Equal(entity.FileName, wordlist.Path);
        Assert.Equal("Credentials", wordlist.Type.Name);
        Assert.Equal("Login", wordlist.Purpose);
        Assert.Equal(3_000_000_000L, wordlist.Total);
    }

    [Fact]
    public void FromEntity_MissingName_UsesFileNameWithoutExtension()
    {
        var settings = new RuriLibSettingsService(tempDir);
        var factory = new WordlistFactory(settings);
        var entity = new WordlistEntity
        {
            FileName = Path.Combine(tempDir, "emails.txt"),
            Type = "Emails"
        };

        var wordlist = factory.FromEntity(entity);

        Assert.Equal("emails", wordlist.Name);
    }

    [Fact]
    public void FromEntity_UnknownType_ThrowsInvalidWordlistTypeException()
    {
        var settings = new RuriLibSettingsService(tempDir);
        var factory = new WordlistFactory(settings);
        var entity = new WordlistEntity { Type = "Unknown" };

        Assert.Throws<InvalidWordlistTypeException>(() => factory.FromEntity(entity));
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }
}
