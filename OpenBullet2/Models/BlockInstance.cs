namespace OpenBullet2.Models
{
    public class BlockInstance
    {
        public bool Disabled { get; set; } = false;
        public BlockInfo Info { get; set; }
        public BlockSettings Settings { get; set; }
        public string OutputVariable { get; set; }
    }
}
