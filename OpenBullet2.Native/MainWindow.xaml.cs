using MahApps.Metro.Controls;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.Utils;
using OpenBullet2.Native.Views.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private UpdateService updateService;

        private Home homePage;
        private Proxies proxiesPage;
        private Wordlists wordlistsPage;
        private OBSettings obSettingsPage;
        private RLSettings rlSettingsPage;
        private Plugins pluginsPage;
        private About aboutPage;
        private Page currentPage;

        public MainWindow()
        {
            InitializeComponent();

            updateService = SP.GetService<UpdateService>();
            Title = $"OpenBullet 2 - {updateService.CurrentVersion} [{updateService.CurrentVersionType}]";
        }

        public void Init()
        {
            homePage = new();
            proxiesPage = new();
            wordlistsPage = new();
            obSettingsPage = new();
            rlSettingsPage = new();
            pluginsPage = new();
            aboutPage = new();

            ChangePage(homePage, menuOptionHome);
        }

        // We recreate the homepage each time to display updated announcements
        private void OpenHomePage(object sender, MouseEventArgs e)
        {
            homePage = new Home();
            ChangePage(homePage, menuOptionHome);
        }

        private void OpenJobsPage(object sender, MouseEventArgs e) { }
        private void OpenMonitorPage(object sender, MouseEventArgs e) { }
        private void OpenProxiesPage(object sender, MouseEventArgs e) => ChangePage(proxiesPage, menuOptionProxies);
        private void OpenWordlistsPage(object sender, MouseEventArgs e) => ChangePage(wordlistsPage, menuOptionWordlists);
        private void OpenConfigsPage(object sender, MouseEventArgs e) { }
        private void OpenHitsPage(object sender, MouseEventArgs e) { }
        private void OpenPluginsPage(object sender, MouseEventArgs e) => ChangePage(pluginsPage, menuOptionPlugins);
        private void OpenOBSettingsPage(object sender, MouseEventArgs e) => ChangePage(obSettingsPage, menuOptionOBSettings);
        private void OpenRLSettingsPage(object sender, MouseEventArgs e) => ChangePage(rlSettingsPage, menuOptionRLSettings);
        private void OpenAboutPage(object sender, MouseEventArgs e) => ChangePage(aboutPage, menuOptionAbout);

        private void ChangePage(Page newPage, Label newLabel)
        {
            currentPage = newPage;
            mainFrame.Content = newPage;

            // Update the selected menu item
            foreach (var child in topMenu.Children)
            {
                var label = child as Label;
                label.Foreground = Brush.Get("ForegroundMain");
            }

            newLabel.Foreground = Brush.Get("ForegroundMenuSelected");
        }

        private void TakeScreenshot(object sender, RoutedEventArgs e)
            => Screenshot.Take((int)Width, (int)Height, (int)Top, (int)Left);
    }
}
