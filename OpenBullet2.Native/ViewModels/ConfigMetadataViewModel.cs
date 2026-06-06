using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.Utils;
using RuriLib.Models.Configs;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenBullet2.Native.ViewModels;

public class ConfigMetadataViewModel : ViewModelBase
{
    private readonly ConfigService configService;
    private Config Config => configService.SelectedConfig
        ?? throw new InvalidOperationException("No config selected");

    public string Name
    {
        get => Config.Metadata.Name;
        set
        {
            Config.Metadata.Name = value;
            OnPropertyChanged();
        }
    }

    public string Author
    {
        get => Config.Metadata.Author;
        set
        {
            Config.Metadata.Author = value;
            OnPropertyChanged();
        }
    }

    public string Category
    {
        get => Config.Metadata.Category;
        set
        {
            Config.Metadata.Category = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage? Icon => string.IsNullOrEmpty(Config.Metadata.Base64Image)
        ? null
        : Images.Base64ToBitmapImage(Config.Metadata.Base64Image);

    public ConfigMetadataViewModel(ConfigService configService) => this.configService = configService;

    public void SetIconFromFile(string fileName)
    {
        var bytes = ImageEditor.ToCompatibleFormat(File.ReadAllBytes(fileName));

        var base64 = Convert.ToBase64String(bytes);
        Config.Metadata.Base64Image = base64;
        OnPropertyChanged(nameof(Icon));
    }

    public async Task SetIconFromUrlAsync(string url)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        var bytes = ImageEditor.ToCompatibleFormat(await response.Content.ReadAsByteArrayAsync());

        var base64 = Convert.ToBase64String(bytes);
        Config.Metadata.Base64Image = base64;
        OnPropertyChanged(nameof(Icon));
    }

    public void SetIconFromClipboard(BitmapSource image)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var ms = new MemoryStream();
        encoder.Save(ms);

        var bytes = ImageEditor.ToCompatibleFormat(ms.ToArray());
        var base64 = Convert.ToBase64String(bytes);
        Config.Metadata.Base64Image = base64;
        OnPropertyChanged(nameof(Icon));
    }

    public Task Save() => configService.SaveSelectedConfigAsync();
}
