using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RuriLib.Models.Variables
{
    public class StringVariable : Variable
    {
        private string value;

        public StringVariable(string value)
        {
            this.value = value;
            Type = VariableType.String;
        }

        public override string AsString() => value;
        
        public override int AsInt()
        {
            if (int.TryParse(value, out int result))
                return result;
            else
                throw new InvalidCastException();
        }

        public override bool AsBool()
        {
            if (bool.TryParse(value, out bool result))
                return result;
            else
                throw new InvalidCastException();
        }

        public override byte[] AsByteArray() => Encoding.UTF8.GetBytes(value);

        public override float AsFloat()
        {
            if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                return result;
            else
                throw new InvalidCastException();
        }

        public override List<string> AsListOfStrings() => new List<string> { value };

        public override object AsObject() => value;
    }
}
