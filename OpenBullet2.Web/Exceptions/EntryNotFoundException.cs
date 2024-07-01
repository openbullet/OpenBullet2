namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// An exception that is thrown when an entry was not found.
/// </summary>
public class EntryNotFoundException : ApiException
{
    /// <summary>
    /// Creates an <see cref="EntryNotFoundException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    public EntryNotFoundException(string errorCode, string message)
        : base(errorCode, message)
    {
    }

    /// <summary>
    /// Creates an <see cref="EntryNotFoundException" />.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="id">The id of the entry that was not found</param>
    /// <param name="collection">The collection in which the entry was searched</param>
    public EntryNotFoundException(string errorCode, object id, string collection)
        : base(errorCode, $"The requested entry with id {id} was not found in the collection {collection}")
    {
    }
}
