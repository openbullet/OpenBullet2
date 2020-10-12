using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate parameters of a block method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BlockParam : Attribute
    {
        public string name = null;
        public string description = null;
        public string extraInfo = null;
        
        public BlockParam(string description)
        {
            this.description = description;
        }
    }
}
