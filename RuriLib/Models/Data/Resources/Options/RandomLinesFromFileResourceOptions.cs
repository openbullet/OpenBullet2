namespace RuriLib.Models.Data.Resources.Options
{
    public class RandomLinesFromFileResourceOptions : ConfigResourceOptions
    {
        /// <summary>
        /// The location of the file on disk.
        /// </summary>
        public string Location { get; set; } = "resource.txt";

        /// <summary>
        /// Whether to skip empty lines when taking lines from the file.
        /// </summary>
        public bool IgnoreEmptyLines { get; set; } = true;

        /// <summary>
        /// True to only take lines that haven't been taken yet, false
        /// to take completely random lines each time (never runs out of lines).
        /// </summary>
        public bool Unique { get; set; } = false;
    }
}
