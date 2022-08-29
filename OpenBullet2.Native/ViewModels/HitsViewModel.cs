using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OpenBullet2.Native.ViewModels
{
    public class HitsViewModel : ViewModelBase
    {
        private readonly OpenBulletSettingsService obSettingsService;
        private readonly IHitRepository hitRepo;
        private bool initialized;

        private ObservableCollection<HitEntity> hitsCollection;
        public ObservableCollection<HitEntity> HitsCollection
        {
            get => hitsCollection;
            private set
            {
                hitsCollection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public int Total => ((CollectionView)CollectionViewSource.GetDefaultView(HitsCollection)).Count;

        private string searchString = string.Empty;
        public string SearchString
        {
            get => searchString;
            set
            {
                searchString = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(HitsCollection).Refresh();
                OnPropertyChanged(nameof(Total));
            }
        }

        public IEnumerable<string> ConfigNames => new string[] { "All" }.Concat(
            HitsCollection.GroupBy(h => h.ConfigName).Select(g => g.First().ConfigName));

        private string configFilter = "All";
        public string ConfigFilter
        {
            get => configFilter;
            set
            {
                configFilter = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(HitsCollection).Refresh();
                OnPropertyChanged(nameof(Total));
            }
        }

        public IEnumerable<string> HitTypes => new string[] { "All" }.Concat(
            HitsCollection.GroupBy(h => h.Type).Select(g => g.First().Type));
        
        private string typeFilter = "All";
        public string TypeFilter
        {
            get => typeFilter;
            set
            {
                typeFilter = value;
                OnPropertyChanged();
                CollectionViewSource.GetDefaultView(HitsCollection).Refresh();
                OnPropertyChanged(nameof(Total));
            }
        }

        public HitsViewModel()
        {
            obSettingsService = SP.GetService<OpenBulletSettingsService>();
            hitRepo = SP.GetService<IHitRepository>();
            HitsCollection = new ObservableCollection<HitEntity>();
        }

        public async Task Initialize()
        {
            if (!initialized)
            {
                await RefreshList();
                initialized = true;
            }
        }

        public void HookFilters()
        {
            var view = (CollectionView)CollectionViewSource.GetDefaultView(HitsCollection);
            view.Filter = HitsFilter;
        }

        private bool HitsFilter(object item)
        {
            var hit = item as HitEntity;
            var captureOk = string.IsNullOrEmpty(searchString) || hit.CapturedData.Contains(searchString, StringComparison.OrdinalIgnoreCase);
            var configOk = configFilter == "All" || hit.ConfigName == configFilter;
            var typeOk = typeFilter == "All" || hit.Type == typeFilter;

            return captureOk && configOk && typeOk;
        }

        public async Task RefreshList()
        {
            try
            {
                // TODO: Make this not fail when hits are being written and we try to read them!
                // A.k.a. make this use another repo, not the singleton, and refresh it when new hits come in
                var items = await hitRepo.GetAll().ToListAsync();
                HitsCollection = new ObservableCollection<HitEntity>(items);
                OnPropertyChanged(nameof(Total));
                HookFilters();
            }
            catch
            {

            }
        }

        public Task Update(HitEntity hit) => hitRepo.Update(hit);

        public async Task Delete(IEnumerable<HitEntity> hits)
        {
            await hitRepo.Delete(hits);
            await RefreshList();
            OnPropertyChanged(nameof(Total));
        }

        public void Purge()
        {
            HitsCollection.Clear();
            hitRepo.Purge();
            OnPropertyChanged(nameof(Total));
        }

        public async Task<int> DeleteDuplicates()
        {
            var duplicates = HitsCollection
                .GroupBy(h => h.GetHashCode(obSettingsService.Settings.GeneralSettings.IgnoreWordlistNameOnHitsDedupe))
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(h => h.Date)
                .Reverse().Skip(1)).ToList();

            await hitRepo.Delete(duplicates);
            await RefreshList();

            return duplicates.Count;
        }

        public override void UpdateViewModel()
        {
            _ = RefreshList();
            base.UpdateViewModel();
        }
    }
}
