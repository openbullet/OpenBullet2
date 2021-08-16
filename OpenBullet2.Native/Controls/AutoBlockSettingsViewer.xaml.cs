using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using System;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for AutoBlockSettingsViewer.xaml
    /// </summary>
    public partial class AutoBlockSettingsViewer : UserControl
    {
        private readonly AutoBlockSettingsViewerViewModel vm;

        public AutoBlockSettingsViewer(BlockViewModel blockVM)
        {
            if (blockVM.Block is not AutoBlockInstance)
            {
                throw new Exception("Wrong block type for this UC");
            }

            vm = new AutoBlockSettingsViewerViewModel(blockVM);
            DataContext = vm;

            InitializeComponent();
            CreateControls();
        }

        private void CreateControls()
        {
            foreach (var setting in vm.BlockVM.Block.Settings)
            {
                UserControl viewer = setting.Value.FixedSetting switch
                {
                    StringSetting => new StringSettingViewer { Setting = setting.Value },
                    IntSetting => new IntSettingViewer { Setting = setting.Value },
                    FloatSetting => new FloatSettingViewer { Setting = setting.Value },
                    EnumSetting => new EnumSettingViewer { Setting = setting.Value },
                    ByteArraySetting => new ByteArraySettingViewer { Setting = setting.Value },
                    BoolSetting => new BoolSettingViewer { Setting = setting.Value },
                    ListOfStringsSetting => new ListOfStringsSettingViewer { Setting = setting.Value },
                    DictionaryOfStringsSetting => new DictionaryOfStringsSettingViewer { Setting = setting.Value },
                    _ => null
                };

                if (viewer is not null)
                {
                    settingsPanel.Children.Add(viewer);
                }
            }
        }
    }

    public class AutoBlockSettingsViewerViewModel : ViewModelBase
    {
        public BlockViewModel BlockVM { get; init; }
        public AutoBlockInstance Block => BlockVM.Block as AutoBlockInstance;

        public string NameAndId => $"{BlockVM.Block.ReadableName} ({BlockVM.Block.Id})";

        public string Label
        {
            get => BlockVM.Label;
            set
            {
                BlockVM.Label = value;
                OnPropertyChanged();
            }
        }

        public bool SafeMode
        {
            get => Block.Safe;
            set
            {
                Block.Safe = value;
                OnPropertyChanged();
            }
        }

        public bool HasReturnValue => Block.Descriptor.ReturnType is not null;
        public string ReturnValueType => $"Output variable ({Block.Descriptor.ReturnType})";

        public string OutputVariable
        {
            get => Block.OutputVariable;
            set
            {
                Block.OutputVariable = value;
                OnPropertyChanged();
            }
        }

        public AutoBlockSettingsViewerViewModel(BlockViewModel block)
        {
            BlockVM = block;
        }
    }
}
