using OpenBullet2.Core.Repositories;
using OpenBullet2.Native.ViewModels;

namespace OpenBullet2.Native.Services
{
    public class ViewModelsService
    {
        public WordlistsViewModel Wordlists { get; set; }

        public ViewModelsService(IWordlistRepository wordlistRepo)
        {
            Wordlists = new WordlistsViewModel(wordlistRepo);
        }
    }
}
