using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace OpenBullet2.Native
{
    /// <summary>
    /// Interaction logic for MainDialog.xaml
    /// </summary>
    public partial class MainDialog : MetroWindow
    {
        public MainDialog(Page content, string title, bool canResize = false)
        {
            InitializeComponent();

            Content = content;
            Title = title;
            ResizeMode = canResize ? System.Windows.ResizeMode.CanResize : System.Windows.ResizeMode.NoResize;
        }
    }
}
