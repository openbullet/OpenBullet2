using System;

namespace RuriLib.Models.Data
{
    public class DataRule
    {
        public string SliceName;
        public string RegexToMatch;

        public DataRule(string sliceName, string regexToMatch = null)
        {
            SliceName = sliceName ?? throw new ArgumentNullException(nameof(sliceName));
            RegexToMatch = regexToMatch ?? throw new ArgumentNullException(nameof(regexToMatch));
        }
    }
}
