using System.Collections.Generic;

namespace RuriLib.Models.Variables
{
    public class ListOfStringsVariable : Variable
    {
        private readonly List<string> value;

        public ListOfStringsVariable(List<string> value)
        {
            this.value = value;
            Type = VariableType.ListOfStrings;
        }

        public override string AsString() => value == null 
            ? "null" 
            : "[" + string.Join(", ", value) + "]";

        public override List<string> AsListOfStrings() => value;

        public override object AsObject() => value;
    }
}
