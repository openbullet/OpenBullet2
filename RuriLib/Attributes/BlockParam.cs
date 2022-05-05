using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a parameter of a method decorated with the <see cref="Block"/>
    /// attribute that can add information about the parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BlockParam : Attribute
    {
        /// <summary>
        /// The name of the parameter. If not specified, a name will automatically be 
        /// generated from the name of the parameter.
        /// </summary>
        public string name = null;

        /// <summary>
        /// The description of what the parameter does.
        /// </summary>
        public string description = null;

        /// <summary>
        /// Provides additional information to a block parameter.
        /// </summary>
        /// <param name="name">The name of the parameter. If not specified, a name will automatically be 
        /// generated from the name of the parameter.</param>
        /// <param name="description">The description of what the parameter does.</param>
        public BlockParam(string name = null, string description = null)
        {
            this.name = name;
            this.description = description;
        }
    }
}
