namespace RuriLib.Models.Blocks.Custom.HttpRequest.Multipart
{
    public class FileHttpContent : MyHttpContent
    {
        public string FileName { get; set; }

        public FileHttpContent(string name, string fileName, string contentType)
        {
            Name = name;
            FileName = fileName;
            ContentType = contentType;
        }
    }
}
