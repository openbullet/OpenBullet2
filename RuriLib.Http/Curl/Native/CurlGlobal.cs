namespace RuriLib.Http.Curl.Native;

internal static class CurlGlobal
{
    private static readonly object Lock = new();
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        lock (Lock)
        {
            if (initialized)
            {
                return;
            }

            // CURL_GLOBAL_DEFAULT is CURL_GLOBAL_SSL | CURL_GLOBAL_WIN32 (3).
            // libcurl requires process-level initialization before easy handles
            // are used; repeated calls are avoided here for predictable startup.
            CurlNativeMethods.ThrowIfError(
                CurlNativeMethods.GlobalInit(3),
                "curl_global_init");

            initialized = true;
        }
    }
}
