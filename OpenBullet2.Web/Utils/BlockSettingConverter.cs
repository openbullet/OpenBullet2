using AutoMapper;
using OpenBullet2.Web.Dtos.Config.Blocks;
using RuriLib.Models.Blocks.Settings;

namespace OpenBullet2.Web.Utils;

internal class BlockSettingConverter : ITypeConverter<BlockSettingDto, BlockSetting>
{
    public BlockSetting Convert(BlockSettingDto source, BlockSetting destination, ResolutionContext context) =>
        destination;
}
