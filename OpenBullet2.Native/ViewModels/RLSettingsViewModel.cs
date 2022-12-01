using RuriLib.Functions.Captchas;
using RuriLib.Models.Settings;
using RuriLib.Parallelization;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Native.ViewModels
{
    public class RLSettingsViewModel : ViewModelBase
    {
        private RuriLibSettingsService service;
        private GeneralSettings General => service.RuriLibSettings.GeneralSettings;
        private ProxySettings Proxy => service.RuriLibSettings.ProxySettings;
        private CaptchaSettings Captcha => service.RuriLibSettings.CaptchaSettings;
        private PuppeteerSettings Puppeteer => service.RuriLibSettings.PuppeteerSettings;
        private SeleniumSettings Selenium => service.RuriLibSettings.SeleniumSettings;

        public event Action<CaptchaServiceType> CaptchaServiceChanged;

        public RLSettingsViewModel()
        {
            service = SP.GetService<RuriLibSettingsService>();
        }

        public IEnumerable<ParallelizerType> ParallelizerTypes => Enum.GetValues(typeof(ParallelizerType)).Cast<ParallelizerType>();

        public ParallelizerType ParallelizerType
        {
            get => General.ParallelizerType;
            set
            {
                General.ParallelizerType = value;
                OnPropertyChanged();
            }
        }

        public bool LogJobActivityToFile
        {
            get => General.LogJobActivityToFile;
            set
            {
                General.LogJobActivityToFile = value;
                OnPropertyChanged();
            }
        }
        
        public bool LogAllResults
        {
            get => General.LogAllResults;
            set
            {
                General.LogAllResults = value;
                OnPropertyChanged();
            }
        }

        public bool EnableBotLogging
        {
            get => General.EnableBotLogging;
            set
            {
                General.EnableBotLogging = value;
                OnPropertyChanged();
            }
        }

        public bool VerboseMode
        {
            get => General.VerboseMode;
            set
            {
                General.VerboseMode = value;
                OnPropertyChanged();
            }
        }

        public bool RestrictBlocksToCWD
        {
            get => General.RestrictBlocksToCWD;
            set
            {
                General.RestrictBlocksToCWD = value;
                OnPropertyChanged();
            }
        }

        public bool UseCustomUserAgentsList
        {
            get => General.UseCustomUserAgentsList;
            set
            {
                General.UseCustomUserAgentsList = value;
                OnPropertyChanged();
            }
        }

        public List<string> UserAgents
        {
            get => General.UserAgents;
            set
            {
                General.UserAgents = value;
                OnPropertyChanged();
            }
        }

        public int ProxyConnectTimeoutMilliseconds
        {
            get => Proxy.ProxyConnectTimeoutMilliseconds;
            set
            {
                Proxy.ProxyConnectTimeoutMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public int ProxyReadWriteTimeoutMilliseconds
        {
            get => Proxy.ProxyReadWriteTimeoutMilliseconds;
            set
            {
                Proxy.ProxyReadWriteTimeoutMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public List<string> GlobalBanKeys
        {
            get => Proxy.GlobalBanKeys;
            set
            {
                Proxy.GlobalBanKeys = value;
                OnPropertyChanged();
            }
        }

        public List<string> GlobalRetryKeys
        {
            get => Proxy.GlobalRetryKeys;
            set
            {
                Proxy.GlobalRetryKeys = value;
                OnPropertyChanged();
            }
        }

        public int CaptchaTimeoutSeconds
        {
            get => Captcha.TimeoutSeconds;
            set
            {
                Captcha.TimeoutSeconds = value;
                OnPropertyChanged();
            }
        }

        public int CaptchaPollingIntervalMilliseconds
        {
            get => Captcha.PollingIntervalMilliseconds;
            set
            {
                Captcha.PollingIntervalMilliseconds = value;
                OnPropertyChanged();
            }
        }

        public bool CheckBalanceBeforeSolving
        {
            get => Captcha.CheckBalanceBeforeSolving;
            set
            {
                Captcha.CheckBalanceBeforeSolving = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<CaptchaServiceType> CaptchaServiceTypes => Enum.GetValues(typeof(CaptchaServiceType)).Cast<CaptchaServiceType>();

        public CaptchaServiceType CurrentCaptchaService
        {
            get => Captcha.CurrentService;
            set
            {
                Captcha.CurrentService = value;
                OnPropertyChanged();
                CaptchaServiceChanged?.Invoke(value);
            }
        }

        public string AntiCaptchaApiKey
        {
            get => Captcha.AntiCaptchaApiKey;
            set
            {
                Captcha.AntiCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string AZCaptchaApiKey
        {
            get => Captcha.AZCaptchaApiKey;
            set
            {
                Captcha.AZCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }
        
        public string CapMonsterHost
        {
            get => Captcha.CapMonsterHost;
            set
            {
                Captcha.CapMonsterHost = value;
                OnPropertyChanged();
            }
        }

        public int CapMonsterPort
        {
            get => Captcha.CapMonsterPort;
            set
            {
                Captcha.CapMonsterPort = value;
                OnPropertyChanged();
            }
        }

        public string CaptchasDotIoApiKey
        {
            get => Captcha.CaptchasDotIoApiKey;
            set
            {
                Captcha.CaptchasDotIoApiKey = value;
                OnPropertyChanged();
            }
        }

        public string CustomTwoCaptchaApiKey
        {
            get => Captcha.CustomTwoCaptchaApiKey;
            set
            {
                Captcha.CustomTwoCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string CustomTwoCaptchaDomain
        {
            get => Captcha.CustomTwoCaptchaDomain;
            set
            {
                Captcha.CustomTwoCaptchaDomain = value;
                OnPropertyChanged();
            }
        }

        public int CustomTwoCaptchaPort
        {
            get => Captcha.CustomTwoCaptchaPort;
            set
            {
                Captcha.CustomTwoCaptchaPort = value;
                OnPropertyChanged();
            }
        }

        public bool CustomTwoCaptchaOverrideHostHeader
        {
            get => Captcha.CustomTwoCaptchaOverrideHostHeader;
            set
            {
                Captcha.CustomTwoCaptchaOverrideHostHeader = value;
                OnPropertyChanged();
            }
        }

        public string DeathByCaptchaUsername
        {
            get => Captcha.DeathByCaptchaUsername;
            set
            {
                Captcha.DeathByCaptchaUsername = value;
                OnPropertyChanged();
            }
        }

        public string DeathByCaptchaPassword
        {
            get => Captcha.DeathByCaptchaPassword;
            set
            {
                Captcha.DeathByCaptchaPassword = value;
                OnPropertyChanged();
            }
        }

        public string DeCaptcherUsername
        {
            get => Captcha.DeCaptcherUsername;
            set
            {
                Captcha.DeCaptcherUsername = value;
                OnPropertyChanged();
            }
        }

        public string DeCaptcherPassword
        {
            get => Captcha.DeCaptcherPassword;
            set
            {
                Captcha.DeCaptcherPassword = value;
                OnPropertyChanged();
            }
        }

        public string ImageTyperzApiKey
        {
            get => Captcha.ImageTyperzApiKey;
            set
            {
                Captcha.ImageTyperzApiKey = value;
                OnPropertyChanged();
            }
        }

        public string RuCaptchaApiKey
        {
            get => Captcha.RuCaptchaApiKey;
            set
            {
                Captcha.RuCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string SolveCaptchaApiKey
        {
            get => Captcha.SolveCaptchaApiKey;
            set
            {
                Captcha.SolveCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string SolveRecaptchaApiKey
        {
            get => Captcha.SolveRecaptchaApiKey;
            set
            {
                Captcha.SolveRecaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string TrueCaptchaApiKey
        {
            get => Captcha.TrueCaptchaApiKey;
            set
            {
                Captcha.TrueCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string TrueCaptchaUsername
        {
            get => Captcha.TrueCaptchaUsername;
            set
            {
                Captcha.TrueCaptchaUsername = value;
                OnPropertyChanged();
            }
        }

        public string TwoCaptchaApiKey
        {
            get => Captcha.TwoCaptchaApiKey;
            set
            {
                Captcha.TwoCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string NineKWApiKey
        {
            get => Captcha.NineKWApiKey;
            set
            {
                Captcha.NineKWApiKey = value;
                OnPropertyChanged();
            }
        }

        public string CustomAntiCaptchaApiKey
        {
            get => Captcha.CustomAntiCaptchaApiKey;
            set
            {
                Captcha.CustomAntiCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string CustomAntiCaptchaDomain
        {
            get => Captcha.CustomAntiCaptchaDomain;
            set
            {
                Captcha.CustomAntiCaptchaDomain = value;
                OnPropertyChanged();
            }
        }

        public int CustomAntiCaptchaPort
        {
            get => Captcha.CustomAntiCaptchaPort;
            set
            {
                Captcha.CustomAntiCaptchaPort = value;
                OnPropertyChanged();
            }
        }

        public string AnyCaptchaApiKey
        {
            get => Captcha.AnyCaptchaApiKey;
            set
            {
                Captcha.AnyCaptchaApiKey = value;
                OnPropertyChanged();
            }
        }

        public string CapSolverApiKey
        {
            get => Captcha.CapSolverApiKey;
            set
            {
                Captcha.CapSolverApiKey = value;
                OnPropertyChanged();
            }
        }

        public string PuppeteerChromeBinaryLocation
        {
            get => Puppeteer.ChromeBinaryLocation;
            set
            {
                Puppeteer.ChromeBinaryLocation = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<SeleniumBrowserType> SeleniumBrowserTypes => Enum.GetValues(typeof(SeleniumBrowserType)).Cast<SeleniumBrowserType>();

        public SeleniumBrowserType SeleniumBrowserType
        {
            get => Selenium.BrowserType;
            set
            {
                Selenium.BrowserType = value;
                OnPropertyChanged();
            }
        }

        public string SeleniumChromeBinaryLocation
        {
            get => Selenium.ChromeBinaryLocation;
            set
            {
                Selenium.ChromeBinaryLocation = value;
                OnPropertyChanged();
            }
        }

        public string SeleniumFirefoxBinaryLocation
        {
            get => Selenium.FirefoxBinaryLocation;
            set
            {
                Selenium.FirefoxBinaryLocation = value;
                OnPropertyChanged();
            }
        }

        public Task<decimal> CheckCaptchaBalance() => CaptchaServiceFactory.GetService(Captcha).GetBalanceAsync();

        public Task Save() => service.Save();

        public void Reset()
        {
            service.RuriLibSettings = new GlobalSettings();

            // Call OnPropertyChanged on all public properties
            foreach (var property in GetType().GetProperties())
            {
                OnPropertyChanged(property.Name);
            }
        }
    }
}
