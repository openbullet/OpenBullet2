using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a class that contains methods decorated with the <see cref="Block"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BlockCategory : Attribute
    {
        public string name = null;
        public string description;
        public string backgroundColor;
        public string foregroundColor;

        public BlockCategory(string name, string description, string backgroundColor = "#fff",
            string foregroundColor = "#000")
        {
            this.name = name;
            this.description = description;
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
        }
    }
}
