typedef int CURLcode;
typedef int CURLINFO;
typedef void CURL;

CURLcode curl_easy_getinfo(CURL *curl, CURLINFO info, ...);
CURLcode curl_easy_setopt(CURL *curl, int option, ...);

CURLcode ob2_curl_easy_getinfo_long(CURL *curl, CURLINFO info, long *value)
{
    return curl_easy_getinfo(curl, info, value);
}

CURLcode ob2_curl_easy_setopt_long(CURL *curl, int option, long value)
{
    return curl_easy_setopt(curl, option, value);
}

CURLcode ob2_curl_easy_setopt_ptr(CURL *curl, int option, void *value)
{
    return curl_easy_setopt(curl, option, value);
}
