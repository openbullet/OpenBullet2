using OpenBullet2.Core.Entities;
using RuriLib.Exceptions;
using RuriLib.Models.Data;
using RuriLib.Services;
using System.IO;
using System.Linq;

namespace OpenBullet2.Core.Models.Data;

/// <summary>
/// A factory that creates a <see cref="Wordlist"/> from a <see cref="WordlistEntity"/>.
/// </summary>
public class WordlistFactory(RuriLibSettingsService ruriLibSettings)
{
    private readonly RuriLibSettingsService ruriLibSettings = ruriLibSettings;

    /// <summary>
    /// Creates a <see cref="Wordlist"/> from a <see cref="WordlistEntity"/>.
    /// </summary>
    public Wordlist FromEntity(WordlistEntity entity)
    {
        var wordlistType = ruriLibSettings.Environment.WordlistTypes
            .FirstOrDefault(w => w.Name == entity.Type) ?? throw new InvalidWordlistTypeException(entity.Type ?? string.Empty);
        var wordlist = new Wordlist(
            entity.Name ?? Path.GetFileNameWithoutExtension(entity.FileName) ?? string.Empty,
            entity.FileName,
            wordlistType,
            entity.Purpose,
            false)
        {
            Id = entity.Id,
            Total = entity.Total
        };

        return wordlist;
    }
}
