using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace OpenBullet2.Native
{
    /// <summary>
    /// Interaction logic for MainDialog.xaml
    /// </summary>
    public partial class MainDialog : MetroWindow
    {
        public MainDialog(Page content, string title)
        {
            InitializeComponent();

            Content = content;
            Title = title;
        }
    }
}
