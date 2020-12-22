using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a method that can be turned into an auto block.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Block : Attribute
    {
        public string name = null;
        public string description = null;
        public string extraInfo = null;

        public Block(string description)
        {
            this.description = description;
        }
    }
}
