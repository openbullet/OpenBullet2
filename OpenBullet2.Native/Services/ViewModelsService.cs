using OpenBullet2.Native.ViewModels;

namespace OpenBullet2.Native.Services
{
    public class ViewModelsService
    {
        public ProxiesViewModel Proxies { get; set; }
        public WordlistsViewModel Wordlists { get; set; }

        public ViewModelsService()
        {
            Proxies = new ProxiesViewModel();
            Wordlists = new WordlistsViewModel();
        }
    }
}
