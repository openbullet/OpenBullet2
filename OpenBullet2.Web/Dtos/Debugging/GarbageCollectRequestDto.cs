namespace OpenBullet2.Web.Dtos.Debugging;

/// <summary>
/// DTO used to specify how to perform garbage collection.
/// </summary>
public class GarbageCollectRequestDto
{
    /// <summary>
    /// The number of the oldest generation to be GC'd. -1 for all.
    /// </summary>
    public int Generations { get; set; }

    /// <summary>
    /// The garbage collection mode.
    /// </summary>
    public GCCollectionMode Mode { get; set; } = GCCollectionMode.Default;

    /// <summary>
    /// True to perform a blocking GC, false to perform it whenever it's possible in the background.
    /// </summary>
    public bool Blocking { get; set; }

    /// <summary>
    /// True to compact the small object heap, false to sweep only.
    /// </summary>
    public bool Compacting { get; set; }
}
