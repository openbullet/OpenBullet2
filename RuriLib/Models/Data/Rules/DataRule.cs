using System;

namespace RuriLib.Models.Data.Rules
{
    public abstract class DataRule
    {
        public bool Invert { get; set; } = false;
        public string SliceName { get; set; } = string.Empty;

        public virtual bool IsSatisfied(string value)
            => throw new NotImplementedException();
    }
}
