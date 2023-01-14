namespace OpenBullet2.Web.Exceptions;

public class EntryNotFoundException : ApiException
{
    public EntryNotFoundException(ErrorCode errorCode, object id, string collection)
        : base(errorCode, $"The requested entry with id {id} was not found in the collection {collection}")
    {

    }

    public EntryNotFoundException(ErrorCode errorCode, string message)
        : base(errorCode, message)
    {

    }
}
