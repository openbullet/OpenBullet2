using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables
{
    public class BoolVariable : Variable
    {
        private bool value = false;

        public BoolVariable(bool value)
        {
            this.value = value;
            Type = VariableType.Bool;
        }

        public override string AsString() => value.ToString();

        public override int AsInt() => value ? 1 : 0;

        public override bool AsBool() => value;

        public override byte[] AsByteArray() => BitConverter.GetBytes(value);

        public override float AsFloat() => value ? 1 : 0;

        public override List<string> AsListOfStrings() => new List<string> { AsString() };

        public override object AsObject() => value;
    }
}
