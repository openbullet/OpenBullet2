using System;
using System.Collections.Generic;

namespace RuriLib.Models.Data.Resources
{
    public abstract class ConfigResource
    {
        /// <summary>
        /// Takes a single string from the resource.
        /// </summary>
        public virtual string TakeOne()
            => throw new NotImplementedException();

        /// <summary>
        /// Takes multiple elements strings the resource.
        /// </summary>
        public virtual List<string> Take(int amount)
            => throw new NotImplementedException();
    }
}
