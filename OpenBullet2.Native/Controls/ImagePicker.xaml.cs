using Microsoft.Win32;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for ImagePicker.xaml
    /// </summary>
    public partial class ImagePicker : UserControl
    {
        private ImagePickerViewModel vm;
        public event EventHandler<byte[]> ImageChanged;

        public ImagePicker(byte[] imageBytes)
        {
            InitializeComponent();
            vm = new ImagePickerViewModel
            {
                ImageBytes = imageBytes
            };
            DataContext = vm;
        }

        private void OpenImage(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Images | *.ico;*.jpg;*.jpeg;*.png;*.bmp",
                FilterIndex = 1
            };

            ofd.ShowDialog();

            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                try
                {
                    vm.SetImageFromFile(ofd.FileName);
                    ImageChanged?.Invoke(this, vm.ImageBytes);
                }
                catch (Exception ex)
                {
                    Alert.Exception(ex);
                }
            }
        }
    }

    public class ImagePickerViewModel : ViewModelBase
    {
        private byte[] imageBytes;
        public byte[] ImageBytes
        {
            get => imageBytes;
            set
            {
                imageBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Image));
            }
        }

        public BitmapImage Image => ImageBytes is null ? null : Images.BytesToBitmapImage(ImageBytes);

        public void SetImageFromFile(string fileName)
            => ImageBytes = ImageEditor.ToCompatibleFormat(File.ReadAllBytes(fileName));
    }
}
