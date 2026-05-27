namespace RuriLib.Http.Curl.Native;

// Numeric values are libcurl CURLOPT_* constants from curl/curl.h. Their ranges
// encode the expected native argument kind: plain long, pointer/object, callback
// function pointer, or curl_off_t. Keep these in sync with the libcurl headers.
internal enum CurlOption
{
    WriteData = 10001,
    Url = 10002,
    Proxy = 10004,
    ProxyUserPwd = 10006,
    ErrorBuffer = 10010,
    PostFields = 10015,
    HttpHeader = 10023,
    HeaderData = 10029,
    CustomRequest = 10036,
    NoBody = 44,
    NoProgress = 43,
    Post = 47,
    FollowLocation = 52,
    SslVerifyPeer = 64,
    MaxRedirs = 68,
    SslVerifyHost = 81,
    HttpVersion = 84,
    ProxyType = 101,
    NoSignal = 99,
    TimeoutMs = 155,
    ConnectTimeoutMs = 156,
    SslOptions = 216,
    WriteFunction = 20011,
    HeaderFunction = 20079,
    XferInfoFunction = 20219,
    XferInfoData = 10057,
    PostFieldSizeLarge = 30120
}
