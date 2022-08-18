using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Variables
{
    public class DictionaryOfStringsVariable : Variable
    {
        private readonly Dictionary<string, string> value;

        public DictionaryOfStringsVariable(Dictionary<string, string> value)
        {
            this.value = value;
            Type = VariableType.DictionaryOfStrings;
        }

        public override string AsString() => value == null
            ? "null"
            : "{" + string.Join(", ", AsListOfStrings().Select(s => $"({s})")) + "}";

        public override List<string> AsListOfStrings() =>
            value.Select(kvp => $"{kvp.Key}, {kvp.Value}").ToList();

        public override Dictionary<string, string> AsDictionaryOfStrings()
            => value;

        public override object AsObject() => value;
    }
}
