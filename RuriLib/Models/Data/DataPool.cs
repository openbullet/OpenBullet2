using System.Collections.Generic;

namespace RuriLib.Models.Data
{
    public abstract class DataPool
    {
        /// <summary>The IEnumerable of all available data lines.</summary>
        public IEnumerable<string> DataList { get; protected set; }

        /// <summary>The total number of lines.</summary>
        public long Size { get; protected set; }

        /// <summary>The wordlist type for data slicing.</summary>
        public string WordlistType { get; protected set; }

        /// <summary>
        /// Reloads the data from the source.
        /// </summary>
        public virtual void Reload()
        {
            throw new System.NotImplementedException();
        }
    }
}
