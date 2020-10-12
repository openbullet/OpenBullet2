using System.IO;
using System.Linq;

namespace RuriLib.Models.Data.DataPools
{
    public class WordlistDataPool : DataPool
    {
        public Wordlist Wordlist { get; }

        /// <summary>
        /// Creates a DataPool by loading lines from a given <paramref name="wordlist"/>.
        /// </summary>
        public WordlistDataPool(Wordlist wordlist)
        {
            Wordlist = wordlist;
            DataList = File.ReadLines(wordlist.Path);
            Size = DataList.Count();
            WordlistType = wordlist.Type.Name;
        }
    }
}
