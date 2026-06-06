using Mapster;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Blocks;
using OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;
using OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;
using OpenBullet2.Web.Dtos.Config.Blocks.Parameters;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Dtos.ConfigDebugger;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Dtos.ProxyGroup;
using OpenBullet2.Web.Dtos.Settings;
using OpenBullet2.Web.Dtos.Shared;
using OpenBullet2.Web.Dtos.User;
using OpenBullet2.Web.Dtos.Wordlist;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Debugger;
using RuriLib.Models.Environment;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Settings;
using System.Reflection;
using Action = RuriLib.Models.Jobs.Monitor.Actions.Action;
using Endpoint = OpenBullet2.Core.Models.Sharing.Endpoint;
using GeneralSettings = OpenBullet2.Core.Models.Settings.GeneralSettings;
using ProxySettings = RuriLib.Models.Settings.ProxySettings;

namespace OpenBullet2.Web.Utils;

internal static class WebMapperConfig
{
    internal static TypeAdapterConfig Create()
    {
        PolyDtoCache.Scan();

        var config = new TypeAdapterConfig();
        RegisterAttributeMappings(config);
        RegisterExplicitMappings(config);
        return config;
    }

    private static void RegisterAttributeMappings(TypeAdapterConfig config)
    {
        var assembly = typeof(WebMapperConfig).Assembly;

        foreach (var type in assembly.GetTypes())
        {
            foreach (var mapsFrom in type.GetCustomAttributes<MapsFromAttribute>())
            {
                if (mapsFrom.AutoMap)
                {
                    config.NewConfig(mapsFrom.SourceType, type);
                }
            }

            foreach (var mapsTo in type.GetCustomAttributes<MapsToAttribute>())
            {
                if (mapsTo.AutoMap)
                {
                    config.NewConfig(type, mapsTo.DestinationType);
                }
            }
        }
    }

    private static void RegisterExplicitMappings(TypeAdapterConfig config)
    {
        AddTwoWay<OpenBulletSettings, OpenBulletSettingsDto>(config);
        AddTwoWay<SecuritySettings, OBSecuritySettingsDto>(config);
        AddTwoWay<GeneralSettings, OBGeneralSettingsDto>(config);
        AddTwoWay<CustomizationSettings, OBCustomizationSettingsDto>(config);
        AddTwoWay<ProxyCheckTarget, ProxyCheckTargetDto>(config);
        AddTwoWay<ConfigSettings, ConfigSettingsDto>(config);
        AddTwoWay<RuriLib.Models.Configs.Settings.GeneralSettings, ConfigGeneralSettingsDto>(config);
        AddTwoWay<RuriLib.Models.Configs.Settings.ProxySettings, ConfigProxySettingsDto>(config);
        AddTwoWay<InputSettings, ConfigInputSettingsDto>(config);
        AddTwoWay<BrowserSettings, ConfigBrowserSettingsDto>(config);
        AddTwoWay<BrowserGhostCursorSettings, ConfigGhostCursorSettingsDto>(config);
        AddTwoWay<ScriptSettings, ConfigScriptSettingsDto>(config);
        AddTwoWay<CustomInput, CustomInputDto>(config);
        AddTwoWay<SimpleDataRule, SimpleDataRuleDto>(config);
        AddTwoWay<RegexDataRule, RegexDataRuleDto>(config);
        AddTwoWay<LinesFromFileResourceOptions, LinesFromFileResourceDto>(config);
        AddTwoWay<RandomLinesFromFileResourceOptions, RandomLinesFromFileResourceDto>(config);
        AddTwoWay<Endpoint, EndpointDto>(config);

        config.NewConfig<CreateGuestDto, GuestEntity>()
            .MapWith(src => WebMappingMethods.ToGuestEntity(src));

        config.NewConfig<UpdateGuestInfoDto, GuestEntity>()
            .Map(dest => dest.AllowedAddresses, src => string.Join(',', src.AllowedAddresses));

        config.NewConfig<UpdateGuestPasswordDto, GuestEntity>()
            .Map(dest => dest.PasswordHash, src =>
                BCrypt.Net.BCrypt.HashPassword(src.Password, BCrypt.Net.SaltRevision.Revision2B));

        config.NewConfig<GuestEntity, GuestDto>()
            .MapWith(src => WebMappingMethods.ToGuestDto(src));

        config.NewConfig<GuestEntity, OwnerDto>();

        config.NewConfig<EnvironmentSettings, EnvironmentSettingsDto>();
        config.NewConfig<OpenBulletSettings, SafeOpenBulletSettingsDto>();
        config.NewConfig<GeneralSettings, SafeOBGeneralSettingsDto>();
        config.NewConfig<CustomizationSettings, SafeOBCustomizationSettingsDto>();

        config.NewConfig<RecordEntity, RecordDto>();
        config.NewConfig<HitEntity, HitDto>();
        config.NewConfig<UpdateHitDto, HitEntity>();
        config.NewConfig<CreateHitDto, HitEntity>()
            .MapWith(src => WebMappingMethods.ToHitEntity(src));

        config.NewConfig<WordlistEntity, WordlistDto>()
            .MapWith(src => WebMappingMethods.ToWordlistDto(src));

        config.NewConfig<UpdateWordlistInfoDto, WordlistEntity>()
            .Map(dest => dest.Type, src => src.WordlistType);

        config.NewConfig<ProxyEntity, ProxyDto>()
            .MapWith(src => WebMappingMethods.ToProxyDto(src));

        config.NewConfig<ProxyGroupEntity, ProxyGroupDto>();
        config.NewConfig<CreateProxyGroupDto, ProxyGroupEntity>();
        config.NewConfig<UpdateProxyGroupDto, ProxyGroupEntity>();

        config.NewConfig<ProxyCheckJob, ProxyCheckJobOverviewDto>()
            .Map(dest => dest.Progress, src => src.Progress < 0 ? 0 : src.Progress);

        config.NewConfig<CreateProxyCheckJobDto, ProxyCheckJobOptions>()
            .MapWith(src => WebMappingMethods.ToProxyCheckJobOptions(src, config));
        config.NewConfig<UpdateProxyCheckJobDto, ProxyCheckJobOptions>()
            .MapWith(src => WebMappingMethods.ToProxyCheckJobOptions(src, config));
        config.NewConfig<ProxyCheckJobOptions, ProxyCheckJobOptionsDto>()
            .MapWith(src => WebMappingMethods.ToProxyCheckJobOptionsDto(src, config));

        config.NewConfig<CreateMultiRunJobDto, MultiRunJobOptions>()
            .MapWith(src => WebMappingMethods.ToMultiRunJobOptions(src, config));
        config.NewConfig<UpdateMultiRunJobDto, MultiRunJobOptions>()
            .MapWith(src => WebMappingMethods.ToMultiRunJobOptions(src, config));
        config.NewConfig<MultiRunJobOptions, MultiRunJobOptionsDto>()
            .MapWith(src => WebMappingMethods.ToMultiRunJobOptionsDto(src, config));

        config.NewConfig<Config, ConfigInfoDto>()
            .MapWith(src => WebMappingMethods.ToConfigInfoDto(src));
        config.NewConfig<Config, ConfigDto>();
        config.NewConfig<UpdateConfigDto, Config>();
        config.NewConfig<UpdateConfigMetadataDto, ConfigMetadata>();
        config.NewConfig<ConfigMetadata, ConfigMetadataDto>();
        config.NewConfig<DataSettings, ConfigDataSettingsDto>()
            .MapWith(src => WebMappingMethods.ToConfigDataSettingsDto(src, config));
        config.NewConfig<ConfigDataSettingsDto, DataSettings>()
            .MapWith(src => WebMappingMethods.ToDataSettings(src, config));

        config.NewConfig<PaginatedHitFiltersDto, HitFiltersDto>();

        config.NewConfig<TriggeredAction, TriggeredActionDto>()
            .MapWith(src => WebMappingMethods.ToTriggeredActionDto(src, config));
        config.NewConfig<CreateTriggeredActionDto, TriggeredAction>()
            .MapWith(src => WebMappingMethods.ToTriggeredAction(src, config));

        config.NewConfig<TimeElapsedTriggerDto, TimeElapsedTrigger>()
            .MapWith(src => WebMappingMethods.ToTimeElapsedTrigger(src));
        config.NewConfig<TimeRemainingTriggerDto, TimeRemainingTrigger>()
            .MapWith(src => WebMappingMethods.ToTimeRemainingTrigger(src));
        config.NewConfig<TimeElapsedTrigger, TimeElapsedTriggerDto>()
            .MapWith(src => WebMappingMethods.ToTimeElapsedTriggerDto(src));
        config.NewConfig<TimeRemainingTrigger, TimeRemainingTriggerDto>()
            .MapWith(src => WebMappingMethods.ToTimeRemainingTriggerDto(src));
        config.NewConfig<WaitActionDto, WaitAction>()
            .MapWith(src => WebMappingMethods.ToWaitAction(src));
        config.NewConfig<WaitAction, WaitActionDto>()
            .MapWith(src => WebMappingMethods.ToWaitActionDto(src));
        config.NewConfig<SetRelativeStartConditionActionDto, SetRelativeStartConditionAction>()
            .MapWith(src => WebMappingMethods.ToSetRelativeStartConditionAction(src));
        config.NewConfig<SetRelativeStartConditionAction, SetRelativeStartConditionActionDto>()
            .MapWith(src => WebMappingMethods.ToSetRelativeStartConditionActionDto(src));

        config.NewConfig<BlockInstance, BlockInstanceDto>()
            .MapWith(src => WebMappingMethods.ToBlockInstanceDto(src, config));
        config.NewConfig<BlockSetting, BlockSettingDto>()
            .MapWith(src => BlockSettingMapper.ToDto(src));
        config.NewConfig<Keychain, KeychainDto>()
            .MapWith(src => WebMappingMethods.ToKeychainDto(src, config));
        config.NewConfig<MultipartRequestParams, MultipartRequestParamsDto>()
            .MapWith(src => WebMappingMethods.ToMultipartRequestParamsDto(src, config));
        config.NewConfig<BlockDescriptor, BlockDescriptorDto>()
            .MapWith(src => WebMappingMethods.ToBlockDescriptorDto(src, config));
        config.NewConfig<EnumParameter, EnumBlockParameterDto>()
            .MapWith(src => WebMappingMethods.ToEnumBlockParameterDto(src));

        config.NewConfig<BlockCategory, BlockCategoryDto>();
        config.NewConfig<DbgStartRequestDto, DebuggerOptions>();
    }

    private static void AddTwoWay<TLeft, TRight>(TypeAdapterConfig config)
    {
        config.NewConfig<TLeft, TRight>();
        config.NewConfig<TRight, TLeft>();
    }
}
