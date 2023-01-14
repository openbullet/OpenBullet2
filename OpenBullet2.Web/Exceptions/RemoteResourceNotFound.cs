namespace OpenBullet2.Web.Exceptions;

public class RemoteResourceNotFound : ApiException
{
    public RemoteResourceNotFound(ErrorCode errorCode, string message) 
        : base(errorCode, message)
    {
    }

    public RemoteResourceNotFound(ErrorCode errorCode,
        string resource, string uri) : 
        base(errorCode, $"Could not find the remote resource '{resource}' at {uri}")
    {

    }
}
