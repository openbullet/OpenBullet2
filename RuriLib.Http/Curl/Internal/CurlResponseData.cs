namespace RuriLib.Http.Curl.Internal;

internal sealed record CurlResponseData(int StatusCode, byte[] Body, string[] Headers);
