using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a block that can display images.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class BlockImage : Attribute
    {
        /// <summary>
        /// The unique id of the image.
        /// </summary>
        public string id = null;

        /// <summary>
        /// The max width in pixels.
        /// </summary>
        public int maxWidth = 300;

        /// <summary>
        /// The max height in pixels.
        /// </summary>
        public int maxHeight = 300;

        /// <summary>
        /// Defines a block image with a given <paramref name="id"/>.
        /// </summary>
        public BlockImage(string id)
        {
            this.id = id;
        }
    }
}
