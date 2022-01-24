using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a parameter of type <see cref="byte[]"/> that
    /// is supposed to be an image.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AsImage : Attribute
    {

    }
}
