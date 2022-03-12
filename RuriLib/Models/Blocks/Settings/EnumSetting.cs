using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RuriLib.Models.Blocks.Settings
{
    public class EnumSetting : Setting
    {
        private Type enumType = null;
        public Type EnumType
        { 
            get => enumType;
            set
            {
                enumType = value;

                // Populate the enum values dictionary (used to have nicer enum names to display)
                foreach (var name in enumType.GetEnumNames())
                {
                    var fi = enumType.GetField(name);

                    if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
                    {
                        enumValues[attributes.First().Description] = name;
                    }
                    else
                    {
                        enumValues[name] = name;
                    }
                }
            }
        }
        public string Value { get; set; }

        public IEnumerable<string> PrettyNames => enumValues.Keys;
        public string PrettyName => enumValues.First(kvp => kvp.Value == Value).Key;
        
        private readonly Dictionary<string, string> enumValues = new();

        public void SetFromPrettyName(string prettyName) => Value = enumValues[prettyName];
    }
}
