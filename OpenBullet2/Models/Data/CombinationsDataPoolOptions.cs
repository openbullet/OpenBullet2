namespace OpenBullet2.Models.Data
{
    public class CombinationsDataPoolOptions : DataPoolOptions
    {
        public string CharSet { get; set; } = "0123456789";
        public int Length { get; set; } = 4;
        public string WordlistType { get; set; } = "Default";
    }
}
