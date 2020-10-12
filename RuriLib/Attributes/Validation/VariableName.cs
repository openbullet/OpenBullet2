using System;

namespace RuriLib.Attributes.Validation
{
    /// <summary>
    /// Attribute used to decorate parameters of a block method to indicate its value must
    /// be a valid variable name according to the C# language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class VariableName : Attribute
    {
        public string defaultVariableName = null;

        public VariableName()
        {

        }
    }
}
