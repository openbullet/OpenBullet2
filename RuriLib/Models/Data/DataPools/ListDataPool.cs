using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Data.DataPools
{
    public class ListDataPool : DataPool
    {
        /// <summary>
        /// Creates a DataPool given an IEnumerable <paramref name="list"/> and counts the amount of lines.
        /// </summary>
        public ListDataPool(IEnumerable<string> list, string wordlistType = "Default")
        {
            DataList = list;
            Size = DataList.Count();
            WordlistType = wordlistType;
        }
    }
}
