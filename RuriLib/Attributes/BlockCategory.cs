using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a class that contains methods decorated with the <see cref="Block"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BlockCategory : Attribute
    {
        /// <summary>
        /// The name of the category.
        /// </summary>
        public string name = null;

        /// <summary>
        /// The common features of blocks that are grouped in this category.
        /// </summary>
        public string description;

        /// <summary>
        /// The background color of the category when displayed in a UI, as an HTML color string.
        /// </summary>
        public string backgroundColor;

        /// <summary>
        /// The foreground color of the category when displayed in a UI, as an HTML color string.
        /// </summary>
        public string foregroundColor;

        /// <summary>
        /// Creates a <see cref="BlockCategory"/> attribute given its <paramref name="name"/>,
        /// <paramref name="description"/> and colors.
        /// </summary>
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
