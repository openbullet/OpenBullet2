using OpenBullet2.Native.ViewModels;
using System;
using System.IO;
using System.Windows.Controls;

namespace OpenBullet2.Native.Views.Dialogs;

/// <summary>
/// Interaction logic for ShowChangelogDialog.xaml
/// </summary>
public partial class ShowChangelogDialog : Page
{
    private readonly ChangelogViewModel vm;

    public ShowChangelogDialog()
    {
        InitializeComponent();
        vm = new ChangelogViewModel();
        DataContext = vm;
    }

    public class ChangelogViewModel : ViewModelBase
    {
        private string text = "Loading...";
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }

        public ChangelogViewModel()
        {
            try
            {
                using var stream = typeof(ShowChangelogDialog).Assembly
                    .GetManifestResourceStream("OpenBullet2.Changelog.md")
                    ?? throw new InvalidOperationException("The bundled changelog could not be found");
                using var reader = new StreamReader(stream);
                Text = reader.ReadToEnd();
            }
            catch
            {
                Text = "Could not retrieve the changelog";
            }
        }
    }
}
