using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls;

/// <summary>
/// Interaction logic for MultipartContentViewer.xaml
/// </summary>
public partial class MultipartContentViewer : UserControl
{
    public HttpContentSettingsGroup MultipartContent { get; init; }

    public event EventHandler? OnDeleted;

    public MultipartContentViewer(HttpContentSettingsGroup content)
    {
        MultipartContent = content;
        DataContext = new MultipartContentViewerViewModel(content);

        InitializeComponent();
        BindSettings();
    }

    private void BindSettings()
    {
        nameViewer.Setting = MultipartContent.Name;
        contentTypeViewer.Setting = MultipartContent.ContentType;
        payloadHost.Content = CreatePayloadViewer(MultipartContent);
    }

    private void Delete(object sender, RoutedEventArgs e) => OnDeleted?.Invoke(this, EventArgs.Empty);

    private static UserControl CreatePayloadViewer(HttpContentSettingsGroup content)
        => content switch
        {
            StringHttpContentSettingsGroup stringContent => new StringSettingViewer
            {
                Setting = stringContent.Data
            },
            RawHttpContentSettingsGroup rawContent => new ByteArraySettingViewer
            {
                Setting = rawContent.Data
            },
            FileHttpContentSettingsGroup fileContent => new StringSettingViewer
            {
                Setting = fileContent.FileName
            },
            _ => throw new NotSupportedException()
        };
}

public class MultipartContentViewerViewModel(HttpContentSettingsGroup content) : ViewModelBase
{
    public string Title => content switch
    {
        StringHttpContentSettingsGroup => "Multipart String Content",
        RawHttpContentSettingsGroup => "Multipart Raw Content",
        FileHttpContentSettingsGroup => "Multipart File Content",
        _ => throw new NotSupportedException()
    };
}
