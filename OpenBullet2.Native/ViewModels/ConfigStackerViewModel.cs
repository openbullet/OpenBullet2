using OpenBullet2.Core.Services;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigStackerViewModel : ViewModelBase
    {
        private readonly ConfigService configService;

        public ConfigStackerViewModel()
        {
            configService = SP.GetService<ConfigService>();
        }

        public override void UpdateViewModel()
        {
            base.UpdateViewModel();
        }
    }
}
