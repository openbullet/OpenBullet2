using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuriLib.Models.Variables
{
    public class FloatVariable : Variable
    {
        private float value = 0;

        public FloatVariable(float value)
        {
            this.value = value;
            Type = VariableType.Float;
        }

        public override string AsString() => value.ToString(CultureInfo.InvariantCulture);

        public override int AsInt() => (int)value;

        public override bool AsBool()
        {
            if (value == 0)
                return false;
            else if (value == 1)
                return true;
            else
                throw new InvalidCastException();
        }

        public override byte[] AsByteArray() => BitConverter.GetBytes(value);

        public override float AsFloat() => value;

        public override List<string> AsListOfStrings() => new List<string> { AsString() };

        public override object AsObject() => value;
    }
}
