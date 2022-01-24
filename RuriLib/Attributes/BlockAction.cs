using RuriLib.Models.Blocks;
using System;

namespace RuriLib.Attributes
{
    /// <summary>
    /// Attribute used to decorate a block that supports an action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BlockAction : Attribute
    {
        /// <summary>
        /// The name of this action.
        /// </summary>
        public string name = null;

        /// <summary>
        /// The action to call. The instanced block will be provided as the
        /// only argument.
        /// </summary>
        public Action<BlockInstance> action = null;

        /// <summary>
        /// Creates a <see cref="BlockAction"/> given the <paramref name="name"/>
        /// and the <paramref name="action"/>.
        /// </summary>
        public BlockAction(string name, Action<BlockInstance> action)
        {
            this.name = name;
            this.action = action;
        }
    }
}
