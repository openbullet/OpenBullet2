namespace OpenBullet2.Core.Models.Jobs
{
    /// <summary>
    /// A wrapper around <see cref="JobOptions"/> for json serialization
    /// when saving it to the database.
    /// </summary>
    public class JobOptionsWrapper
    {
        public JobOptions Options { get; set; }
    }
}
