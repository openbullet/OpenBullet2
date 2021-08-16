using OpenBullet2.Core.Services;
using RuriLib.Models.Blocks;

namespace OpenBullet2.Native.ViewModels
{
    public class ConfigStackerViewModel : ViewModelBase
    {
        private readonly ConfigService configService;

        public ConfigStackerViewModel()
        {
            configService = SP.GetService<ConfigService>();
        }

        public void CreateBlock(BlockDescriptor descriptor)
        {

        }

        public override void UpdateViewModel()
        {
            base.UpdateViewModel();
        }
    }
}
