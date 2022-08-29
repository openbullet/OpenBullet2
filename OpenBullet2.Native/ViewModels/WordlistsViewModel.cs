using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OpenBullet2.Native.ViewModels
{
    public class WordlistsViewModel : ViewModelBase
    {
        private readonly IWordlistRepository wordlistRepo;
        private bool initialized;

        private ObservableCollection<WordlistEntity> wordlistsCollection;
        public ObservableCollection<WordlistEntity> WordlistsCollection
        {
            get => wordlistsCollection;
            private set
            {
                wordlistsCollection = value;
                OnPropertyChanged();
            }
        }

        public int Total => WordlistsCollection.Count;

        private string searchString = string.Empty;
        public string SearchString
        {
            get => searchString;
            set
            {
                searchString = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(WordlistsCollection).Refresh();
                OnPropertyChanged(nameof(Total));
            }
        }

        public WordlistsViewModel()
        {
            wordlistRepo = SP.GetService<IWordlistRepository>();
            WordlistsCollection = new();
        }

        public async Task Initialize()
        {
            if (!initialized)
            {
                await RefreshList();
                initialized = true;
            }
        }

        private void HookFilters()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(WordlistsCollection);
            view.Filter = WordlistsFilter;
        }

        private bool WordlistsFilter(object item) => (item as WordlistEntity).Name.Contains(searchString, StringComparison.OrdinalIgnoreCase);

        public WordlistEntity GetWordlistByName(string name) => WordlistsCollection.First(w => w.Name == name);

        public Task Add(WordlistEntity wordlist)
        {
            if (WordlistsCollection.Any(w => w.FileName == wordlist.FileName))
            {
                throw new Exception($"Wordlist already present: {wordlist.FileName}");
            }

            WordlistsCollection.Add(wordlist);
            return wordlistRepo.Add(wordlist);
        }

        public async Task RefreshList()
        {
            var items = await wordlistRepo.GetAll().ToListAsync();
            WordlistsCollection = new ObservableCollection<WordlistEntity>(items);
            HookFilters();
        }

        public async Task Update(WordlistEntity wordlist) => await wordlistRepo.Update(wordlist);

        public async Task Delete(WordlistEntity wordlist)
        {
            WordlistsCollection.Remove(wordlist);
            await wordlistRepo.Delete(wordlist, false);
            OnPropertyChanged(nameof(Total));
        }

        public void DeleteAll()
        {
            WordlistsCollection.Clear();
            wordlistRepo.Purge();
            OnPropertyChanged(nameof(Total));
        }

        public async Task<int> DeleteNotFound()
        {
            var deleted = 0;

            for (var i = 0; i < WordlistsCollection.Count; i++)
            {
                var wordlist = WordlistsCollection[i];

                if (!File.Exists(wordlist.FileName))
                {
                    await Delete(wordlist);
                    deleted++;
                    i--;
                }
            }

            return deleted;
        }
    }
}
