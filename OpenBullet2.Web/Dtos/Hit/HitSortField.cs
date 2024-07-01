namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// The field to sort hits by.
/// </summary>
public enum HitSortField
{
    /// <summary>
    /// The type of the hit.
    /// </summary>
    Type,
    
    /// <summary>
    /// The hit's data.
    /// </summary>
    Data,
    
    /// <summary>
    /// The name of the config that produced the hit.
    /// </summary>
    ConfigName,
    
    /// <summary>
    /// The date when the hit was found.
    /// </summary>
    Date,
    
    /// <summary>
    /// The name of the wordlist that produced the hit.
    /// </summary>
    WordlistName,
    
    /// <summary>
    /// The proxy that was used when the hit was found.
    /// </summary>
    Proxy,
    
    /// <summary>
    /// The captured data of the hit.
    /// </summary>
    CapturedData
}
