using RuriLib.Models.Hits.HitOutputs;

namespace OpenBullet2.Core.Models.Hits
{
    /// <summary>
    /// Options for a <see cref="FileSystemHitOutput"/>.
    /// </summary>
    public class FileSystemHitOutputOptions : HitOutputOptions
    {
        /// <summary>
        /// The parent directory inside which the text files will be created.
        /// </summary>
        public string BaseDir { get; set; } = "Hits";
    }
}
