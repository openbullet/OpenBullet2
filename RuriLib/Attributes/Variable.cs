using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate parameters of a block method to indicate it should be initialized
    /// as a setting of type variable, optionally with the given default variable name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Variable : Attribute
    {
        public string defaultVariableName = null;

        public Variable()
        {

        }

        public Variable(string defaultVariableName)
        {
            this.defaultVariableName = defaultVariableName;
        }
    }
}
