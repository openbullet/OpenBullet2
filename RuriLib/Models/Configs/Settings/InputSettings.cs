using System.Collections.Generic;

namespace RuriLib.Models.Configs.Settings
{
    public class CustomInput
    {
        public string Description { get; set; } = "";
        public string VariableName { get; set; } = "";
        public string DefaultAnswer { get; set; } = "";
    }

    public class InputSettings
    {
        public List<CustomInput> CustomInputs { get; set; } = new List<CustomInput>();
    }
}
