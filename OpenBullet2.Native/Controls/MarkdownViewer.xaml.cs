using Ganss.XSS;
using Markdig;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Utils;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for MarkdownViewer.xaml
    /// </summary>
    public partial class MarkdownViewer : UserControl
    {
        private string queuedRender = string.Empty;

        public string MarkdownText
        {
            get => (string)GetValue(MarkdownTextProperty);
            set => SetValue(MarkdownTextProperty, value);
        }

        public static readonly DependencyProperty MarkdownTextProperty =
        DependencyProperty.Register(
            nameof(MarkdownText),
            typeof(string),
            typeof(MarkdownViewer),
            new PropertyMetadata(default(string), OnMarkdownTextPropertyChanged));

        private static void OnMarkdownTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as string;
            var source = d as MarkdownViewer;

            if (!string.IsNullOrEmpty(newValue))
            {
                var dangerous = Markdown.ToHtml(newValue);
                var html = new HtmlSanitizer().Sanitize(dangerous);
                source.Render(html);
            }
        }

        public MarkdownViewer()
        {
            InitializeComponent();

            // Load the last string queued for render if any
            if (!string.IsNullOrEmpty(queuedRender))
            {
                Render(queuedRender);
            }

            // This is needed to avoid going blind before the page is loaded
            browser.Navigated += (s, e) => browser.Visibility = Visibility.Visible;

            // TODO: Switch to the WinForms WebBrowser, the WPF one is underfeatured
            // disabling the context menu is extremely difficult
        }

        public void Render(string html)
        {
            // If the browser is not loaded yet, queue the render
            if (!browser.IsLoaded)
            {
                queuedRender = html;
                return;
            }

            // Add the styling
            var final = new HtmlStyler(html)
                .WithStyle("background-color", "#222")
                .WithStyle("color", "white")
                .WithStyle("font-family", "Verdana, sans-serif")
                .WithStyle("font-size", $"{(int)FontSize}px")
                .WithStyle("overflow", "hidden")
                .ToString();

            browser.NavigateToString(final);
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
