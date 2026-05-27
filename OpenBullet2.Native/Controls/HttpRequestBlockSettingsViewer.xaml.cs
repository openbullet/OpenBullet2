using OpenBullet2.Native.ViewModels;
using RuriLib.Functions.Http.Options;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Settings;
using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls;

/// <summary>
/// Interaction logic for HttpRequestBlockSettingsViewer.xaml
/// </summary>
public partial class HttpRequestBlockSettingsViewer : UserControl
{
    private readonly HttpRequestBlockSettingsViewerViewModel vm;

    public HttpRequestBlockSettingsViewer(BlockViewModel blockVM)
    {
        if (blockVM.Block is not HttpRequestBlockInstance)
        {
            throw new Exception("Wrong block type for this UC");
        }

        vm = new HttpRequestBlockSettingsViewerViewModel(blockVM);
        vm.ModeChanged += mode =>
        {
            BindSettings();
            tabControl.SelectedIndex = (int)mode;
        };
        DataContext = vm;

        InitializeComponent();
        httpLibrarySetting.ValueChanged += (_, _) => vm.UpdateLibraryDependentSettingsVisibility();

        tabControl.SelectedIndex = (int)vm.Mode;
        BindSettings();
    }

    // TODO: Find a way to automatically scout the visual tree and get the settings viewers by Tag
    // to set their Setting property automatically basing on the Tag instead of doing it manually
    private void BindSettings()
    {
        urlSetting.Setting = vm.HttpRequestBlock.Settings["url"];
        methodSetting.Setting = vm.HttpRequestBlock.Settings["method"];
        httpVersionSetting.Setting = vm.HttpRequestBlock.Settings["httpVersion"];
        autoRedirectSetting.Setting = vm.HttpRequestBlock.Settings["autoRedirect"];
        alwaysSendContentSetting.Setting = vm.HttpRequestBlock.Settings["alwaysSendContent"];
        decodeHtmlSetting.Setting = vm.HttpRequestBlock.Settings["decodeHtml"];

        switch (vm.Mode)
        {
            case HttpRequestMode.Standard:
                var standardParams = (StandardRequestParams)vm.HttpRequestBlock.RequestParams;
                standardContentSetting.Setting = standardParams.Content;
                standardContentTypeSetting.Setting = standardParams.ContentType;
                urlEncodeContentSetting.Setting = vm.HttpRequestBlock.Settings["urlEncodeContent"];
                break;

            case HttpRequestMode.Raw:
                var rawParams = (RawRequestParams)vm.HttpRequestBlock.RequestParams;
                rawContentSetting.Setting = rawParams.Content;
                rawContentTypeSetting.Setting = rawParams.ContentType;
                break;

            case HttpRequestMode.BasicAuth:
                var basicAuthParams = (BasicAuthRequestParams)vm.HttpRequestBlock.RequestParams;
                basicAuthUsernameSetting.Setting = basicAuthParams.Username;
                basicAuthPasswordSetting.Setting = basicAuthParams.Password;
                break;

            case HttpRequestMode.Multipart:
                var multipartParams = (MultipartRequestParams)vm.HttpRequestBlock.RequestParams;
                multipartBoundarySetting.Setting = multipartParams.Boundary;
                BindMultipartContents(multipartParams);
                break;
        }

        customCookiesSetting.Setting = vm.HttpRequestBlock.Settings["customCookies"];
        customHeadersSetting.Setting = vm.HttpRequestBlock.Settings["customHeaders"];
        timeoutMillisecondsSetting.Setting = vm.HttpRequestBlock.Settings["timeoutMilliseconds"];
        maxNumberOfRedirectsSetting.Setting = vm.HttpRequestBlock.Settings["maxNumberOfRedirects"];
        absoluteUriInFirstLineSetting.Setting = vm.HttpRequestBlock.Settings["absoluteUriInFirstLine"];
        readResponseContentSetting.Setting = vm.HttpRequestBlock.Settings["readResponseContent"];
        codePagesEncodingSetting.Setting = vm.HttpRequestBlock.Settings["codePagesEncoding"];
        httpLibrarySetting.Setting = vm.HttpRequestBlock.Settings["httpLibrary"];
        curlImpersonateBrowserProfileSetting.Setting = vm.HttpRequestBlock.Settings["curlImpersonateBrowserProfile"];
        curlUseBrowserHeadersSetting.Setting = vm.HttpRequestBlock.Settings["curlUseBrowserHeaders"];
        securityProtocolSetting.Setting = vm.HttpRequestBlock.Settings["securityProtocol"];
        ignoreCertificateValidationSetting.Setting = vm.HttpRequestBlock.Settings["ignoreCertificateValidation"];
        useCustomCipherSuitesSetting.Setting = vm.HttpRequestBlock.Settings["useCustomCipherSuites"];
        customCipherSuitesSetting.Setting = vm.HttpRequestBlock.Settings["customCipherSuites"];
    }

    private void AddMultipartString(object sender, RoutedEventArgs e)
    {
        vm.AddMultipartStringContent();
        BindMultipartContents((MultipartRequestParams)vm.HttpRequestBlock.RequestParams);
    }

    private void AddMultipartRaw(object sender, RoutedEventArgs e)
    {
        vm.AddMultipartRawContent();
        BindMultipartContents((MultipartRequestParams)vm.HttpRequestBlock.RequestParams);
    }

    private void AddMultipartFile(object sender, RoutedEventArgs e)
    {
        vm.AddMultipartFileContent();
        BindMultipartContents((MultipartRequestParams)vm.HttpRequestBlock.RequestParams);
    }

    private void RemoveMultipartContent(object? sender, EventArgs e)
    {
        if (sender is MultipartContentViewer { MultipartContent: { } content, Parent: Panel panel } viewer)
        {
            vm.RemoveMultipartContent(content);
            panel.Children.Remove(viewer);
        }
    }

    private void BindMultipartContents(MultipartRequestParams multipartParams)
    {
        multipartContentsPanel.Children.Clear();

        foreach (var content in multipartParams.Contents)
        {
            var view = new MultipartContentViewer(content);
            view.OnDeleted += RemoveMultipartContent;
            multipartContentsPanel.Children.Add(view);
        }
    }
}

public class HttpRequestBlockSettingsViewerViewModel(BlockViewModel block) : BlockSettingsViewerViewModel(block)
{
    public HttpRequestBlockInstance HttpRequestBlock => (HttpRequestBlockInstance)Block;

    public bool SafeMode
    {
        get => HttpRequestBlock.Safe;
        set
        {
            HttpRequestBlock.Safe = value;
            OnPropertyChanged();
        }
    }

    public event Action<HttpRequestMode>? ModeChanged;

    private StandardRequestParams cachedStandardParams = new();
    private RawRequestParams cachedRawParams = new();
    private BasicAuthRequestParams cachedBasicAuthParams = new();
    private MultipartRequestParams cachedMultipartParams = new();
    // Seed the per-mode cache from the block's current payload once, so the first
    // mode switch preserves the object that was originally loaded instead of replacing it.
    private bool requestParamsCachePrimed;

    public bool IsCurlImpersonate => HttpLibrarySetting.Value == nameof(HttpLibrary.CurlImpersonate);

    public bool IsNotCurlImpersonate => !IsCurlImpersonate;

    public HttpRequestMode Mode
    {
        get
        {
            PrimeRequestParamsCache();

            return HttpRequestBlock.RequestParams switch
            {
                StandardRequestParams => HttpRequestMode.Standard,
                RawRequestParams => HttpRequestMode.Raw,
                BasicAuthRequestParams => HttpRequestMode.BasicAuth,
                MultipartRequestParams => HttpRequestMode.Multipart,
                _ => throw new NotImplementedException()
            };
        }
        set
        {
            PrimeRequestParamsCache();
            CacheRequestParams(HttpRequestBlock.RequestParams);

            HttpRequestBlock.RequestParams = value switch
            {
                HttpRequestMode.Standard => cachedStandardParams,
                HttpRequestMode.Raw => cachedRawParams,
                HttpRequestMode.BasicAuth => cachedBasicAuthParams,
                HttpRequestMode.Multipart => cachedMultipartParams,
                _ => throw new NotImplementedException()
            };

            ModeChanged?.Invoke(value);
            OnPropertyChanged(nameof(Mode));
            OnPropertyChanged(nameof(StandardMode));
            OnPropertyChanged(nameof(RawMode));
            OnPropertyChanged(nameof(BasicAuthMode));
            OnPropertyChanged(nameof(MultipartMode));
        }
    }

    public bool StandardMode
    {
        get => Mode == HttpRequestMode.Standard;
        set
        {
            if (value)
            {
                Mode = HttpRequestMode.Standard;
            }

            OnPropertyChanged();
        }
    }

    public bool RawMode
    {
        get => Mode == HttpRequestMode.Raw;
        set
        {
            if (value)
            {
                Mode = HttpRequestMode.Raw;
            }

            OnPropertyChanged();
        }
    }

    public bool BasicAuthMode
    {
        get => Mode == HttpRequestMode.BasicAuth;
        set
        {
            if (value)
            {
                Mode = HttpRequestMode.BasicAuth;
            }

            OnPropertyChanged();
        }
    }

    public bool MultipartMode
    {
        get => Mode == HttpRequestMode.Multipart;
        set
        {
            if (value)
            {
                Mode = HttpRequestMode.Multipart;
            }

            OnPropertyChanged();
        }
    }

    public StringHttpContentSettingsGroup AddMultipartStringContent()
    {
        var content = new StringHttpContentSettingsGroup();
        GetMultipartRequestParams().Contents.Add(content);
        return content;
    }

    public RawHttpContentSettingsGroup AddMultipartRawContent()
    {
        var content = new RawHttpContentSettingsGroup();
        GetMultipartRequestParams().Contents.Add(content);
        return content;
    }

    public FileHttpContentSettingsGroup AddMultipartFileContent()
    {
        var content = new FileHttpContentSettingsGroup();
        GetMultipartRequestParams().Contents.Add(content);
        return content;
    }

    public void RemoveMultipartContent(HttpContentSettingsGroup content)
        => GetMultipartRequestParams().Contents.Remove(content);

    public void UpdateLibraryDependentSettingsVisibility()
    {
        OnPropertyChanged(nameof(IsCurlImpersonate));
        OnPropertyChanged(nameof(IsNotCurlImpersonate));
    }

    private MultipartRequestParams GetMultipartRequestParams()
    {
        PrimeRequestParamsCache();

        if (HttpRequestBlock.RequestParams is not MultipartRequestParams multipartRequestParams)
        {
            throw new InvalidOperationException("Multipart content is only available in multipart mode");
        }

        return multipartRequestParams;
    }

    private void PrimeRequestParamsCache()
    {
        if (requestParamsCachePrimed)
        {
            return;
        }

        // The block only holds one RequestParams instance at a time, but the editor
        // lets the user flip between modes and keep each mode's last edited payload alive.
        CacheRequestParams(HttpRequestBlock.RequestParams);
        requestParamsCachePrimed = true;
    }

    private void CacheRequestParams(RequestParams requestParams)
    {
        switch (requestParams)
        {
            case StandardRequestParams standardRequestParams:
                cachedStandardParams = standardRequestParams;
                break;

            case RawRequestParams rawRequestParams:
                cachedRawParams = rawRequestParams;
                break;

            case BasicAuthRequestParams basicAuthRequestParams:
                cachedBasicAuthParams = basicAuthRequestParams;
                break;

            case MultipartRequestParams multipartRequestParams:
                cachedMultipartParams = multipartRequestParams;
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private EnumSetting HttpLibrarySetting
        => (EnumSetting)HttpRequestBlock.Settings["httpLibrary"].FixedSetting!;
}

public enum HttpRequestMode
{
    Standard,
    Raw,
    BasicAuth,
    Multipart
}
