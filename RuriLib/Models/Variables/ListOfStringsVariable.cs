using System;
using System.Collections.Generic;
using System.Text;

namespace RuriLib.Models.Variables
{
    public class ListOfStringsVariable : Variable
    {
        private List<string> value = new List<string>();

        public ListOfStringsVariable(List<string> value)
        {
            this.value = value;
            Type = VariableType.ListOfStrings;
        }

        public override string AsString() => "[" + string.Join(", ", value) + "]";

        public override List<string> AsListOfStrings() => new List<string> { AsString() };
    }
}
