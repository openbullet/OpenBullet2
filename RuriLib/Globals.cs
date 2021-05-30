using RuriLib.Helpers.Blocks;

namespace RuriLib
{
    /// <summary>
    /// Singleton for data that should only be initialized once.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// The repository of all block descriptors loaded from assemblies so far.
        /// </summary>
        public static DescriptorsRepository DescriptorsRepository { get; set; }
            = new DescriptorsRepository();
    }
}
