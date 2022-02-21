using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a parameter of a block method to indicate it should be initialized
    /// as a setting of type interpolated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class Interpolated : Attribute
    {
        public Interpolated()
        {

        }
    }
}
