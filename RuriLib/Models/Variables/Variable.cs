using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables
{
    public abstract class Variable
    {
        public string Name { get; set; } = "variable";
        public bool MarkedForCapture { get; set; } = false;
        public VariableType Type { get; set; } = VariableType.String;

        public virtual string AsString() => throw new InvalidCastException();
        public virtual int AsInt() => throw new InvalidCastException();
        public virtual float AsFloat() => throw new InvalidCastException();
        public virtual bool AsBool() => throw new InvalidCastException();
        public virtual List<string> AsListOfStrings() => throw new InvalidCastException();
        public virtual Dictionary<string, string> AsDictionaryOfStrings() => throw new InvalidCastException();
        public virtual byte[] AsByteArray() => throw new InvalidCastException();
        public virtual object AsObject() => throw new InvalidCastException();
    }
}
