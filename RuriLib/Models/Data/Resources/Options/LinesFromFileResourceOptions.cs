namespace RuriLib.Models.Data.Resources.Options
{
    public class LinesFromFileResourceOptions : ConfigResourceOptions
    {
        /// <summary>
        /// The location of the file on disk.
        /// </summary>
        public string Location { get; set; } = "resource.txt";

        /// <summary>
        /// Whether to go back to the start of the file if there are no more lines to read.
        /// </summary>
        public bool LoopsAround { get; set; } = true;

        /// <summary>
        /// Whether to skip empty lines when taking lines from the file.
        /// </summary>
        public bool IgnoreEmptyLines { get; set; } = true;
    }
}
