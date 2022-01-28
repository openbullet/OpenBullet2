namespace RuriLib.Models.Blocks
{
    public delegate void BlockActionDelegate(BlockInstance block);

    public class BlockActionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BlockActionDelegate Delegate { get; set; }
    }
}
