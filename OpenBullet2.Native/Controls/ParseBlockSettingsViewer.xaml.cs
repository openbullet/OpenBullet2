using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.Parse;
using System;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for ParseBlockSettingsViewer.xaml
    /// </summary>
    public partial class ParseBlockSettingsViewer : UserControl
    {
        private readonly ParseBlockSettingsViewerViewModel vm;

        public ParseBlockSettingsViewer(BlockViewModel blockVM)
        {
            if (blockVM.Block is not ParseBlockInstance)
            {
                throw new Exception("Wrong block type for this UC");
            }

            vm = new ParseBlockSettingsViewerViewModel(blockVM);
            vm.ModeChanged += mode => tabControl.SelectedIndex = (int)mode;
            DataContext = vm;

            InitializeComponent();

            tabControl.SelectedIndex = (int)vm.Mode;
            BindSettings();
        }

        // TODO: Find a way to automatically scout the visual tree and get the settings viewers by Tag
        // to set their Setting property automatically basing on the Tag instead of doing it manually
        private void BindSettings()
        {
            // General
            inputSetting.Setting = vm.ParseBlock.Settings["input"];
            prefixSetting.Setting = vm.ParseBlock.Settings["prefix"];
            suffixSetting.Setting = vm.ParseBlock.Settings["suffix"];
            urlEncodeOutputSetting.Setting = vm.ParseBlock.Settings["urlEncodeOutput"];

            // LR
            leftDelimSetting.Setting = vm.ParseBlock.Settings["leftDelim"];
            rightDelimSetting.Setting = vm.ParseBlock.Settings["rightDelim"];
            caseSensitiveSetting.Setting = vm.ParseBlock.Settings["caseSensitive"];

            // CSS
            cssSelectorSetting.Setting = vm.ParseBlock.Settings["cssSelector"];
            attributeNameSetting.Setting = vm.ParseBlock.Settings["attributeName"];

            // XPath
            xPathSetting.Setting = vm.ParseBlock.Settings["xPath"];

            // Json
            jTokenSetting.Setting = vm.ParseBlock.Settings["jToken"];

            // Regex
            patternSetting.Setting = vm.ParseBlock.Settings["pattern"];
            outputFormatSetting.Setting = vm.ParseBlock.Settings["outputFormat"];
            multiLineSetting.Setting = vm.ParseBlock.Settings["multiLine"];
        }
    }

    public class ParseBlockSettingsViewerViewModel : BlockSettingsViewerViewModel
    {
        public ParseBlockInstance ParseBlock => Block as ParseBlockInstance;

        public bool SafeMode
        {
            get => ParseBlock.Safe;
            set
            {
                ParseBlock.Safe = value;
                OnPropertyChanged();
            }
        }


        public event Action<ParseMode> ModeChanged;

        public string OutputVariable
        {
            get => ParseBlock.OutputVariable;
            set
            {
                ParseBlock.OutputVariable = value;
                OnPropertyChanged();
            }
        }

        public bool Recursive
        {
            get => ParseBlock.Recursive;
            set
            {
                ParseBlock.Recursive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ReturnValueType));
            }
        }

        public bool IsCapture
        {
            get => ParseBlock.IsCapture;
            set
            {
                ParseBlock.IsCapture = value;
                OnPropertyChanged();
            }
        }

        public ParseMode Mode
        {
            get => ParseBlock.Mode;
            set
            {
                ParseBlock.Mode = value;
                ModeChanged?.Invoke(value);
                OnPropertyChanged();
            }
        }

        public bool LRMode
        {
            get => Mode == ParseMode.LR;
            set
            {
                if (value)
                {
                    Mode = ParseMode.LR;
                }

                OnPropertyChanged();
            }
        }

        public bool CSSMode
        {
            get => Mode == ParseMode.CSS;
            set
            {
                if (value)
                {
                    Mode = ParseMode.CSS;
                }

                OnPropertyChanged();
            }
        }

        public bool XPathMode
        {
            get => Mode == ParseMode.XPath;
            set
            {
                if (value)
                {
                    Mode = ParseMode.XPath;
                }

                OnPropertyChanged();
            }
        }

        public bool JsonMode
        {
            get => Mode == ParseMode.Json;
            set
            {
                if (value)
                {
                    Mode = ParseMode.Json;
                }

                OnPropertyChanged();
            }
        }

        public bool RegexMode
        {
            get => Mode == ParseMode.Regex;
            set
            {
                if (value)
                {
                    Mode = ParseMode.Regex;
                }

                OnPropertyChanged();
            }
        }

        public string ReturnValueType => $"Output variable ({(ParseBlock.Recursive ? "ListOfStrings" : "String")})";

        public ParseBlockSettingsViewerViewModel(BlockViewModel block) : base(block)
        {
             
        }
    }
}
