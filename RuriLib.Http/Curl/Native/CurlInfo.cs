namespace RuriLib.Http.Curl.Native;

// Mirrors CURLINFO_* values used with curl_easy_getinfo.
internal enum CurlInfo
{
    // CURLINFO_LONG is 0x200000; CURLINFO_HTTP_VERSION is CURLINFO_LONG + 46.
    HttpVersion = 0x200000 + 46
}
