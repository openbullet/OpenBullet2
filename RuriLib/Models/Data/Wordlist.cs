using RuriLib.Models.Environment;
using System;
using System.IO;
using System.Linq;

namespace RuriLib.Models.Data
{
    public class Wordlist
    {
        public int Id { get; set; }

        /// <summary>The name of the Wordlist.</summary>
        public string Name { get; set; }

        /// <summary>The path where the file is stored on disk. If null, the wordlist doesn't reside on disk.</summary>
        public string Path { get; set; } = null;

        /// <summary>The WordlistType.</summary>
        public WordlistType Type { get; set; }

        /// <summary>The purpose for which the Wordlist should be used.</summary>
        public string Purpose { get; set; }

        /// <summary>The total number of data lines of the file.</summary>
        public int Total { get; set; }

        /// <summary>
        /// Creates an instance of a Wordlist.
        /// </summary>
        /// <param name="name">The name of the Wordlist</param>
        /// <param name="path">The path to the file on disk. Use null if the config only resides in memory.</param>
        /// <param name="type">The WordlistType as a string</param>
        /// <param name="purpose">The purpose of the Wordlist</param>
        /// <param name="countLines">Whether to enumerate the total number of data lines in the Wordlist</param>
        public Wordlist(string name, string path, WordlistType type, string purpose, bool countLines = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = path;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Purpose = purpose ?? "";
            Total = countLines ? Total = File.ReadLines(path).Count() : 0;
        }
    }
}
