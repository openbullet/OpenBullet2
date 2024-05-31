namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// A record of a check made by a Multi Run Job.
/// </summary>
public class RecordDto
{
    /// <summary>
    /// The id of the config that was used to create the record.
    /// </summary>
    public required string ConfigId { get; set; }
    
    /// <summary>
    /// The id of the wordlist that was used to create the record.
    /// </summary>
    public int WordlistId { get; set; }
    
    /// <summary>
    /// The checkpoint, which represents how far into the wordlist the last
    /// check of a Multi Run Job made it, for a given config and wordlist combination.
    /// </summary>
    public int Checkpoint { get; set; }
}
