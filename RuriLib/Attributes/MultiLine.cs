using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a string parameter of a block method to indicate it should be rendered
    /// with a multi-line textbox.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class MultiLine : Attribute
    {
        public MultiLine()
        {

        }
    }
}
