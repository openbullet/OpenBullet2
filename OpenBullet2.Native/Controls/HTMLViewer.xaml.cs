using Ganss.XSS;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Utils;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for MarkdownViewer.xaml
    /// </summary>
    public partial class HTMLViewer : UserControl
    {
        public string HTML
        {
            get => (string)GetValue(HTMLProperty);
            set => SetValue(HTMLProperty, value);
        }

        public static readonly DependencyProperty HTMLProperty =
        DependencyProperty.Register(
            nameof(HTML),
            typeof(string),
            typeof(HTMLViewer),
            new PropertyMetadata(default(string), OnHTMLPropertyChanged));

        private static void OnHTMLPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as string;
            var source = d as HTMLViewer;

            if (!string.IsNullOrEmpty(newValue))
            {
                var html = new HtmlSanitizer().Sanitize(newValue);
                source.Render(html);
            }
        }

        public HTMLViewer()
        {
            InitializeComponent();
            Render(string.Empty);
        }

        public void Render(string html)
        {
            // Add the styling
            var final = new HtmlStyler(html)
                .WithStyle("background-color", "#222")
                .WithStyle("color", "white")
                .WithStyle("font-family", "Verdana, sans-serif")
                .WithStyle("font-size", $"{(int)FontSize}px")
                .ToString();

            browser.DocumentText = final;
        }

        private void Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            // The Uri is not null if the user clicked on a link inside the markdown
            if (e.Uri is not null)
            {
                // Do not propagate
                e.Cancel = true;

                // Handle it with the default browser
                Url.Open(e.Uri.ToString());
            }
        }
    }
}
