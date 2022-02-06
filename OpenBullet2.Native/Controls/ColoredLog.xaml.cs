using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for ColoredLog.xaml
    /// </summary>
    public partial class ColoredLog : UserControl
    {
        public int BufferSize { get; set; } = 30;
        private int count = 0;

        public ColoredLog()
        {
            InitializeComponent();
        }

        public void Append(string message, Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            var block = new TextBlock
            {
                Text = message,
                Foreground = brush
            };

            log.Children.Add(block);
            count++;

            if (count > BufferSize)
            {
                log.Children.RemoveAt(0);
                count--;
            }

            scrollViewer.ScrollToBottom();
        }

        public void Clear()
        {
            log.Children.Clear();
            count = 0;
        }
    }
}
