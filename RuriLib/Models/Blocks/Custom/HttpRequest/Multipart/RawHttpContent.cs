namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class RawHttpContent : MyHttpContent
    {
        public byte[] Data { get; set; }

        public RawHttpContent(string name, byte[] data, string contentType)
        {
            Name = name;
            Data = data;
            ContentType = contentType;
        }
    }
}
