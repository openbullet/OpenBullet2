using RuriLib.Models.Hits.HitOutputs;

namespace OpenBullet2.Core.Models.Hits;

/// <summary>
/// Options for a <see cref="FileSystemHitOutput"/>.
/// </summary>
public class FileSystemHitOutputOptions : HitOutputOptions
{
    /// <summary>
    /// The directory template inside which the text files will be created.
    /// Supports placeholders like &lt;CONFIG&gt;, &lt;WORDLIST&gt; and &lt;DATE&gt;.
    /// </summary>
    public string BaseDir { get; set; } = "UserData/Hits/<CONFIG>/<DATE>";
}
