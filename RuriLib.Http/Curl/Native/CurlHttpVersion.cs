namespace RuriLib.Http.Curl.Native;

// Mirrors CURL_HTTP_VERSION_* values used with CURLOPT_HTTP_VERSION.
internal enum CurlHttpVersion
{
    None = 0,
    Version10 = 1,
    Version11 = 2,
    Version20 = 3,
    Version2Tls = 4,
    Version2PriorKnowledge = 5,
    Version30 = 30,
    Version3Only = 31
}
