namespace OpenBullet2.Web.Exceptions;

/// <summary>
/// A generic API exception.
/// </summary>
public class ApiException : Exception
{
    /// <summary>
    /// Creates an <see cref="ApiException" /> given an
    /// <paramref name="errorCode" /> and a <paramref name="message" />.
    /// </summary>
    public ApiException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// The error code.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <inheritdoc />
    public override string ToString() => Message;
}

/// <summary>
/// Error codes for managed API exceptions.
/// </summary>
public static class ErrorCode
{
    /// <summary>
    /// Internal Server Error.
    /// </summary>
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";

    /// <summary>
    /// Unauthorized access.
    /// </summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>
    /// Unauthorized IP address.
    /// </summary>
    public const string UnauthorizedIpAddress = "UNAUTHORIZED_IP_ADDRESS";
    
    /// <summary>
    /// Validation error.
    /// </summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>
    /// Missing auth token.
    /// </summary>
    public const string MissingAuthToken = "MISSING_AUTH_TOKEN";

    /// <summary>
    /// Invalid auth token.
    /// </summary>
    public const string InvalidAuthToken = "INVALID_AUTH_TOKEN";
    
    /// <summary>
    /// Not authenticated.
    /// </summary>
    public const string NotAuthenticated = "NOT_AUTHENTICATED";

    /// <summary>
    /// Invalid API key.
    /// </summary>
    public const string InvalidApiKey = "INVALID_API_KEY";

    /// <summary>
    /// Invalid username or password.
    /// </summary>
    public const string InvalidCredentials = "INVALID_CREDENTIALS";

    /// <summary>
    /// Only admins can perform this action.
    /// </summary>
    public const string NotAdmin = "NOT_ADMIN";

    /// <summary>
    /// The guest account has expired.
    /// </summary>
    public const string GuestAccountExpired = "EXPIRED_GUEST_ACCOUNT";

    /// <summary>
    /// The guest account is invalid (probably missing from the database).
    /// </summary>
    public const string InvalidGuestAccount = "INVALID_GUEST_ACCOUNT";
    
    /// <summary>
    /// The user has no permission to perform this action.
    /// </summary>
    public const string InvalidRole = "INVALID_ROLE";

    /// <summary>
    /// Local file not found.
    /// </summary>
    public const string FileNotFound = "FILE_NOT_FOUND";

    /// <summary>
    /// File outside allowed path.
    /// </summary>
    public const string FileOutsideAllowedPath = "FILE_OUTSIDE_ALLOWED_PATH";

    /// <summary>
    /// Remote resource not found.
    /// </summary>
    public const string RemoteResourceNotFound = "REMOTE_RESOURCE_NOT_FOUND";

    /// <summary>
    /// Remote resource fetch failed.
    /// </summary>
    public const string RemoteResourceFetchFailed = "REMOTE_RESOURCE_FETCH_FAILED";

    /// <summary>
    /// Local file already exists.
    /// </summary>
    public const string FileAlreadyExists = "FILE_ALREADY_EXISTS";

    /// <summary>
    /// Guest user not found.
    /// </summary>
    public const string GuestUserNotFound = "GUEST_USER_NOT_FOUND";

    /// <summary>
    /// A user with the same username already exists.
    /// </summary>
    public const string UsernameTaken = "USERNAME_TAKEN";

    /// <summary>
    /// Plugin not found.
    /// </summary>
    public const string PluginNotFound = "PLUGIN_NOT_FOUND";

    /// <summary>
    /// Wordlist not found.
    /// </summary>
    public const string WordlistNotFound = "WORDLIST_NOT_FOUND";

    /// <summary>
    /// Proxy group not found.
    /// </summary>
    public const string ProxyGroupNotFound = "PROXY_GROUP_NOT_FOUND";

    /// <summary>
    /// Config not found.
    /// </summary>
    public const string ConfigNotFound = "CONFIG_NOT_FOUND";

    /// <summary>
    /// Endpoint not found.
    /// </summary>
    public const string EndpointNotFound = "ENDPOINT_NOT_FOUND";

    /// <summary>
    /// Hit not found.
    /// </summary>
    public const string HitNotFound = "HIT_NOT_FOUND";
    
    /// <summary>
    /// No hits selected.
    /// </summary>
    public const string NoHitsSelected = "NO_HITS_SELECTED";

    /// <summary>
    /// Triggered action not found.
    /// </summary>
    public const string TriggeredActionNotFound = "TRIGGERED_ACTION_NOT_FOUND";

    /// <summary>
    /// Job not found.
    /// </summary>
    public const string JobNotFound = "JOB_NOT_FOUND";

    /// <summary>
    /// Endpoint already exists.
    /// </summary>
    public const string EndpointAlreadyExists = "ENDPOINT_ALREADY_EXISTS";

    /// <summary>
    /// The proxy group is being used in a job.
    /// </summary>
    public const string ProxyGroupInUse = "PROXY_GROUP_IN_USE";

    /// <summary>
    /// The job is not idle.
    /// </summary>
    public const string JobNotIdle = "JOB_NOT_IDLE";

    /// <summary>
    /// Invalid job type.
    /// </summary>
    public const string InvalidJobType = "INVALID_JOB_TYPE";

    /// <summary>
    /// Invalid job configuration.
    /// </summary>
    public const string InvalidJobConfiguration = "INVALID_JOB_CONFIGURATION";

    /// <summary>
    /// Action not allowed for a remote config.
    /// </summary>
    public const string ActionNotAllowedForRemoteConfig = "ACTION_NOT_ALLOWED_FOR_REMOTE_CONFIG";

    /// <summary>
    /// Invalid block id.
    /// </summary>
    public const string InvalidBlockId = "INVALID_BLOCK_ID";

    /// <summary>
    /// The config debugger is not idle.
    /// </summary>
    public const string ConfigDebuggerNotIdle = "CONFIG_DEBUGGER_NOT_IDLE";

    /// <summary>
    /// Missing config id.
    /// </summary>
    public const string MissingConfigId = "MISSING_CONFIG_ID";

    /// <summary>
    /// Missing job id.
    /// </summary>
    public const string MissingJobId = "MISSING_JOB_ID";
    
    /// <summary>
    /// Captcha service error.
    /// </summary>
    public const string CaptchaServiceError = "CAPTCHA_SERVICE_ERROR";
}
