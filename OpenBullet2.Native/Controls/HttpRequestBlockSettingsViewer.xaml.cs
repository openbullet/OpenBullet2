using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using System;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
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
                    standardContentSetting.Setting = (vm.HttpRequestBlock.RequestParams as StandardRequestParams).Content;
                    standardContentTypeSetting.Setting = (vm.HttpRequestBlock.RequestParams as StandardRequestParams).ContentType;
                    urlEncodeContentSetting.Setting = vm.HttpRequestBlock.Settings["urlEncodeContent"];
                    break;

                case HttpRequestMode.Raw:
                    rawContentSetting.Setting = (vm.HttpRequestBlock.RequestParams as RawRequestParams).Content;
                    rawContentTypeSetting.Setting = (vm.HttpRequestBlock.RequestParams as RawRequestParams).ContentType;
                    break;

                case HttpRequestMode.BasicAuth:
                    basicAuthUsernameSetting.Setting = (vm.HttpRequestBlock.RequestParams as BasicAuthRequestParams).Username;
                    basicAuthPasswordSetting.Setting = (vm.HttpRequestBlock.RequestParams as BasicAuthRequestParams).Password;
                    break;

                case HttpRequestMode.Multipart:
                    multipartBoundarySetting.Setting = (vm.HttpRequestBlock.RequestParams as MultipartRequestParams).Boundary;
                    // TODO: write this
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
            securityProtocolSetting.Setting = vm.HttpRequestBlock.Settings["securityProtocol"];
            useCustomCipherSuitesSetting.Setting = vm.HttpRequestBlock.Settings["useCustomCipherSuites"];
            customCipherSuitesSetting.Setting = vm.HttpRequestBlock.Settings["customCipherSuites"];
        }
    }

    public class HttpRequestBlockSettingsViewerViewModel : BlockSettingsViewerViewModel
    {
        public HttpRequestBlockInstance HttpRequestBlock => Block as HttpRequestBlockInstance;

        public bool SafeMode
        {
            get => HttpRequestBlock.Safe;
            set
            {
                HttpRequestBlock.Safe = value;
                OnPropertyChanged();
            }
        }

        public event Action<HttpRequestMode> ModeChanged;

        private StandardRequestParams cachedStandardParams = new();
        private RawRequestParams cachedRawParams = new();
        private BasicAuthRequestParams cachedBasicAuthParams = new();
        private MultipartRequestParams cachedMultipartParams = new();

        public HttpRequestMode Mode
        {
            get => HttpRequestBlock.RequestParams switch
            {
                StandardRequestParams => HttpRequestMode.Standard,
                RawRequestParams => HttpRequestMode.Raw,
                BasicAuthRequestParams => HttpRequestMode.BasicAuth,
                MultipartRequestParams => HttpRequestMode.Multipart,
                _ => throw new NotImplementedException()
            };
            set
            {
                HttpRequestBlock.RequestParams = value switch
                {
                    HttpRequestMode.Standard => cachedStandardParams,
                    HttpRequestMode.Raw => cachedRawParams,
                    HttpRequestMode.BasicAuth => cachedBasicAuthParams,
                    HttpRequestMode.Multipart => cachedMultipartParams,
                    _ => throw new NotImplementedException()
                };

                ModeChanged?.Invoke(value);
                OnPropertyChanged();
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

        public HttpRequestBlockSettingsViewerViewModel(BlockViewModel block) : base(block)
        {

        }
    }

    public enum HttpRequestMode
    {
        Standard,
        Raw,
        BasicAuth,
        Multipart
    }
}
