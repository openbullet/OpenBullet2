using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for KeyViewer.xaml
    /// </summary>
    public partial class KeyViewer : UserControl
    {
        public Key Key { get; init; }

        private readonly KeyViewerViewModel vm;
        public event EventHandler OnDeleted;

        public KeyViewer(Key key)
        {
            Key = key;
            vm = new KeyViewerViewModel(key);
            DataContext = vm;

            InitializeComponent();
            BindSettings(key);
        }

        private void BindSettings(Key key)
        {
            leftSetting.Children.Add(ConvertSetting(key.Left));
            rightSetting.Children.Add(ConvertSetting(key.Right));
        }

        private void Delete(object sender, RoutedEventArgs e) => OnDeleted?.Invoke(this, EventArgs.Empty);

        private static UserControl ConvertSetting(BlockSetting setting)
            => setting.FixedSetting switch
            {
                StringSetting => new StringSettingViewer { Setting = setting },
                BoolSetting => new BoolSettingViewer { Setting = setting },
                IntSetting => new IntSettingViewer { Setting = setting },
                FloatSetting => new FloatSettingViewer { Setting = setting },
                ListOfStringsSetting => new ListOfStringsSettingViewer { Setting = setting },
                DictionaryOfStringsSetting => new DictionaryOfStringsSettingViewer { Setting = setting },
                _ => throw new NotImplementedException(),
            };
    }

    public class KeyViewerViewModel : ViewModelBase
    {
        private readonly Key key;

        public IEnumerable<string> Comparisons => Enum.GetNames((key as dynamic).Comparison.GetType());
        public string Comparison
        {
            get => (key as dynamic).Comparison.ToString();
            set
            {
                switch (key)
                {
                    case StringKey stringKey:
                        stringKey.Comparison = (StrComparison)Enum.Parse(typeof(StrComparison), value);
                        break;

                    case IntKey intKey:
                        intKey.Comparison = (NumComparison)Enum.Parse(typeof(NumComparison), value);
                        break;

                    case BoolKey boolKey:
                        boolKey.Comparison = (BoolComparison)Enum.Parse(typeof(BoolComparison), value);
                        break;

                    case ListKey listKey:
                        listKey.Comparison = (ListComparison)Enum.Parse(typeof(ListComparison), value);
                        break;

                    case DictionaryKey dictKey:
                        dictKey.Comparison = (DictComparison)Enum.Parse(typeof(DictComparison), value);
                        break;

                    case FloatKey floatKey:
                        floatKey.Comparison = (NumComparison)Enum.Parse(typeof(NumComparison), value);
                        break;
                }

                OnPropertyChanged();
            }
        }

        public KeyViewerViewModel(Key key)
        {
            this.key = key;


        }
    }
}
