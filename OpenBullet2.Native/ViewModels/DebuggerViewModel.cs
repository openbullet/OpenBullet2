using RuriLib.Models.Proxies;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Native.ViewModels
{
    public class DebuggerViewModel : ViewModelBase
    {
        private readonly RuriLibSettingsService rlSettingsService;

        private string testData;
        public string TestData
        {
            get => testData;
            set
            {
                testData = value;
                OnPropertyChanged();
            }
        }

        private string wordlistType;
        public string WordlistType
        {
            get => wordlistType;
            set
            {
                wordlistType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> WordlistTypes => rlSettingsService.Environment.WordlistTypes.Select(w => w.Name);

        private bool persistLog;
        public bool PersistLog
        {
            get => persistLog;
            set
            {
                persistLog = value;
                OnPropertyChanged();
            }
        }

        private bool useProxy;
        public bool UseProxy
        {
            get => useProxy;
            set
            {
                useProxy = value;
                OnPropertyChanged();
            }
        }

        private string testProxy;
        public string TestProxy
        {
            get => testProxy;
            set
            {
                testProxy = value;
                OnPropertyChanged();
            }
        }

        private ProxyType proxyType = ProxyType.Http;
        public ProxyType ProxyType
        {
            get => proxyType;
            set
            {
                proxyType = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ProxyType> ProxyTypes => Enum.GetValues(typeof(ProxyType)).Cast<ProxyType>();

        public bool CanStart => true;
        public bool CanStop => false;

        public DebuggerViewModel()
        {
            rlSettingsService = SP.GetService<RuriLibSettingsService>();
            WordlistType = WordlistTypes.First();
        }
    }
}
