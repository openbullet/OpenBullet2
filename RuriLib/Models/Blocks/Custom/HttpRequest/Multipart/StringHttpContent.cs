namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class StringHttpContent : MyHttpContent
    {
        public string Data { get; set; }

        public StringHttpContent(string name, string data, string contentType)
        {
            Name = name;
            Data = data;
            ContentType = contentType;
        }
    }
}
