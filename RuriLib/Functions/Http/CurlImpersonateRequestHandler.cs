namespace RuriLib.Functions.Http;

internal sealed class CurlImpersonateRequestHandler : HttpClientRequestHandler
{
    public CurlImpersonateRequestHandler()
        : base(HttpFactory.GetCurlImpersonateHttpClient)
    {
    }
}
