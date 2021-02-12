namespace RuriLib.Functions.Http.Options
{
    public class BasicAuthHttpRequestOptions : HttpRequestOptions
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
