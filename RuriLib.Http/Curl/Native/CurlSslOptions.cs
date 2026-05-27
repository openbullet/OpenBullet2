namespace RuriLib.Http.Curl.Native;

internal static class CurlSslOptions
{
    // CURLSSLOPT_NATIVE_CA asks libcurl to use the OS certificate store in
    // addition to any configured CA bundle/path.
    public const long NativeCa = 1L << 4;
}
