using BCrypt.Net;
using Mapster;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Blocks;
using OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;
using OpenBullet2.Web.Dtos.Config.Blocks.Keycheck;
using OpenBullet2.Web.Dtos.Config.Blocks.Parameters;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Dtos.Settings;
using OpenBullet2.Web.Dtos.User;
using OpenBullet2.Web.Dtos.Wordlist;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Custom.HttpRequest;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using RuriLib.Models.Jobs.StartConditions;
using System.Text.Json;
using Action = RuriLib.Models.Jobs.Monitor.Actions.Action;
using Endpoint = OpenBullet2.Core.Models.Sharing.Endpoint;
using GeneralSettings = OpenBullet2.Core.Models.Settings.GeneralSettings;

namespace OpenBullet2.Web.Utils;

internal static class WebMappingMethods
{
    internal static GuestEntity ToGuestEntity(CreateGuestDto dto) => new()
    {
        Username = dto.Username,
        AccessExpiration = dto.AccessExpiration,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, SaltRevision.Revision2B),
        AllowedAddresses = string.Join(',', dto.AllowedAddresses)
    };

    internal static GuestDto ToGuestDto(GuestEntity entity) => new()
    {
        Id = entity.Id,
        Username = entity.Username ?? string.Empty,
        AccessExpiration = entity.AccessExpiration,
        AllowedAddresses = (entity.AllowedAddresses ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToList()
    };

    internal static HitEntity ToHitEntity(CreateHitDto dto) => new()
    {
        Data = dto.Data,
        CapturedData = dto.CapturedData,
        Proxy = dto.Proxy ?? string.Empty,
        Date = dto.Date ?? DateTime.Now,
        Type = dto.Type,
        ConfigId = dto.ConfigId,
        ConfigName = dto.ConfigName ?? string.Empty,
        ConfigCategory = dto.ConfigCategory ?? string.Empty,
        WordlistId = dto.WordlistId,
        WordlistName = dto.WordlistName ?? string.Empty
    };

    internal static WordlistDto ToWordlistDto(WordlistEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name ?? string.Empty,
        FilePath = entity.FileName ?? string.Empty,
        Purpose = entity.Purpose ?? string.Empty,
        LineCount = entity.Total,
        WordlistType = entity.Type ?? string.Empty,
        Owner = entity.Owner is null
            ? null
            : new OwnerDto
            {
                Id = entity.Owner.Id,
                Username = entity.Owner.Username ?? string.Empty
            }
    };

    internal static ProxyDto ToProxyDto(ProxyEntity entity) => new()
    {
        Id = entity.Id,
        Host = entity.Host ?? string.Empty,
        Port = entity.Port,
        Username = entity.Username,
        Password = entity.Password,
        Type = entity.Type,
        Ping = entity.Ping,
        Quality = entity.Quality,
        Country = entity.Country,
        Status = entity.Status,
        GroupId = entity.Group?.Id ?? -1,
        GroupName = entity.Group?.Name ?? string.Empty,
        LastChecked = entity.LastChecked == default ? null : entity.LastChecked
    };

    internal static ProxyCheckJobOptions ToProxyCheckJobOptions(
        CreateProxyCheckJobDto dto, TypeAdapterConfig config) => new()
        {
            Bots = dto.Bots,
            Name = dto.Name,
            StartCondition = PolyMapper.MapBetween<TimeStartConditionDto, StartCondition>(
            (JsonElement)dto.StartCondition!, config) ?? new RelativeTimeStartCondition(),
            GroupId = dto.GroupId,
            CheckOnlyUntested = dto.CheckOnlyUntested,
            Target = dto.Target?.Adapt<ProxyCheckTarget>(config) ?? new ProxyCheckTarget(),
            TimeoutMilliseconds = dto.TimeoutMilliseconds,
            UseProxyJudge = dto.UseProxyJudge,
            CheckOutput = PolyMapper.MapBetween<ProxyCheckOutputOptionsDto, ProxyCheckOutputOptions>(
            (JsonElement)dto.CheckOutput!, config)!
        };

    internal static ProxyCheckJobOptions ToProxyCheckJobOptions(
        UpdateProxyCheckJobDto dto, TypeAdapterConfig config) => new()
        {
            Bots = dto.Bots,
            Name = dto.Name,
            StartCondition = PolyMapper.MapBetween<TimeStartConditionDto, StartCondition>(
            (JsonElement)dto.StartCondition!, config) ?? new RelativeTimeStartCondition(),
            GroupId = dto.GroupId,
            CheckOnlyUntested = dto.CheckOnlyUntested,
            Target = dto.Target?.Adapt<ProxyCheckTarget>(config) ?? new ProxyCheckTarget(),
            TimeoutMilliseconds = dto.TimeoutMilliseconds,
            UseProxyJudge = dto.UseProxyJudge,
            CheckOutput = PolyMapper.MapBetween<ProxyCheckOutputOptionsDto, ProxyCheckOutputOptions>(
            (JsonElement)dto.CheckOutput!, config)!
        };

    internal static MultiRunJobOptions ToMultiRunJobOptions(
        CreateMultiRunJobDto dto, TypeAdapterConfig config) => new()
        {
            ConfigId = dto.ConfigId,
            Name = dto.Name,
            StartCondition = PolyMapper.MapBetween<TimeStartConditionDto, StartCondition>(
            (JsonElement)dto.StartCondition!, config) ?? new RelativeTimeStartCondition(),
            Bots = dto.Bots,
            Skip = dto.Skip,
            ProxyMode = dto.ProxyMode,
            ShuffleProxies = dto.ShuffleProxies,
            NoValidProxyBehaviour = dto.NoValidProxyBehaviour,
            ProxyBanTimeSeconds = dto.ProxyBanTimeSeconds,
            MarkAsToCheckOnAbort = dto.MarkAsToCheckOnAbort,
            NeverBanProxies = dto.NeverBanProxies,
            NeverMarkProxiesAsBad = dto.NeverMarkProxiesAsBad,
            ConcurrentProxyMode = dto.ConcurrentProxyMode,
            PeriodicReloadIntervalSeconds = dto.PeriodicReloadIntervalSeconds,
            DataPool = PolyMapper.MapBetween<DataPoolOptionsDto, DataPoolOptions>(
            (JsonElement)dto.DataPool!, config)!,
            ProxySources = PolyMapper.MapBetween<ProxySourceOptionsDto, ProxySourceOptions>(
            dto.ProxySources, config),
            HitOutputs = PolyMapper.MapBetween<HitOutputOptionsDto, HitOutputOptions>(
            dto.HitOutputs, config)
        };

    internal static MultiRunJobOptions ToMultiRunJobOptions(
        UpdateMultiRunJobDto dto, TypeAdapterConfig config) => new()
        {
            ConfigId = dto.ConfigId,
            Name = dto.Name,
            StartCondition = PolyMapper.MapBetween<TimeStartConditionDto, StartCondition>(
            (JsonElement)dto.StartCondition!, config) ?? new RelativeTimeStartCondition(),
            Bots = dto.Bots,
            Skip = dto.Skip,
            ProxyMode = dto.ProxyMode,
            ShuffleProxies = dto.ShuffleProxies,
            NoValidProxyBehaviour = dto.NoValidProxyBehaviour,
            ProxyBanTimeSeconds = dto.ProxyBanTimeSeconds,
            MarkAsToCheckOnAbort = dto.MarkAsToCheckOnAbort,
            NeverBanProxies = dto.NeverBanProxies,
            NeverMarkProxiesAsBad = dto.NeverMarkProxiesAsBad,
            ConcurrentProxyMode = dto.ConcurrentProxyMode,
            PeriodicReloadIntervalSeconds = dto.PeriodicReloadIntervalSeconds,
            DataPool = PolyMapper.MapBetween<DataPoolOptionsDto, DataPoolOptions>(
            (JsonElement)dto.DataPool!, config)!,
            ProxySources = PolyMapper.MapBetween<ProxySourceOptionsDto, ProxySourceOptions>(
            dto.ProxySources, config),
            HitOutputs = PolyMapper.MapBetween<HitOutputOptionsDto, HitOutputOptions>(
            dto.HitOutputs, config)
        };

    internal static ProxyCheckJobOptionsDto ToProxyCheckJobOptionsDto(
        ProxyCheckJobOptions options, TypeAdapterConfig config) => new()
        {
            Name = options.Name,
            StartCondition = PolyMapper.MapFrom(options.StartCondition, config)!,
            Bots = options.Bots,
            GroupId = options.GroupId,
            CheckOnlyUntested = options.CheckOnlyUntested,
            Target = options.Target.Adapt<ProxyCheckTargetDto>(config),
            TimeoutMilliseconds = options.TimeoutMilliseconds,
            UseProxyJudge = options.UseProxyJudge,
            CheckOutput = PolyMapper.MapFrom(options.CheckOutput, config)!
        };

    internal static MultiRunJobOptionsDto ToMultiRunJobOptionsDto(
        MultiRunJobOptions options, TypeAdapterConfig config) => new()
        {
            Name = options.Name,
            StartCondition = PolyMapper.MapFrom(options.StartCondition, config)!,
            ConfigId = options.ConfigId,
            Bots = options.Bots,
            Skip = options.Skip,
            ProxyMode = options.ProxyMode,
            ShuffleProxies = options.ShuffleProxies,
            NoValidProxyBehaviour = options.NoValidProxyBehaviour,
            ProxyBanTimeSeconds = options.ProxyBanTimeSeconds,
            MarkAsToCheckOnAbort = options.MarkAsToCheckOnAbort,
            NeverBanProxies = options.NeverBanProxies,
            NeverMarkProxiesAsBad = options.NeverMarkProxiesAsBad,
            ConcurrentProxyMode = options.ConcurrentProxyMode,
            PeriodicReloadIntervalSeconds = options.PeriodicReloadIntervalSeconds,
            DataPool = PolyMapper.MapFrom(options.DataPool, config)!,
            ProxySources = PolyMapper.MapAllFrom(options.ProxySources, config),
            HitOutputs = PolyMapper.MapAllFrom(options.HitOutputs, config)
        };

    internal static ConfigInfoDto ToConfigInfoDto(Config config) => new()
    {
        Id = config.Id,
        Author = config.Metadata.Author,
        Category = config.Metadata.Category,
        CreationDate = config.Metadata.CreationDate,
        LastModified = config.Metadata.LastModified,
        Name = config.Metadata.Name,
        Base64Image = config.Metadata.Base64Image,
        IsRemote = config.IsRemote,
        Mode = config.Mode,
        AllowedWordlistTypes = config.Settings.DataSettings.AllowedWordlistTypes.ToList(),
        NeedsProxies = config.Settings.ProxySettings.UseProxies,
        SuggestedBots = config.Settings.GeneralSettings.SuggestedBots,
        Dangerous = false
    };

    internal static ConfigDataSettingsDto ToConfigDataSettingsDto(
        DataSettings settings, TypeAdapterConfig config) => new()
        {
            AllowedWordlistTypes = settings.AllowedWordlistTypes,
            UrlEncodeDataAfterSlicing = settings.UrlEncodeDataAfterSlicing,
            DataRules = new DataRulesDto
            {
                Simple = settings.DataRules.OfType<SimpleDataRule>()
                .Select(r => r.Adapt<SimpleDataRuleDto>(config))
                .ToList(),
                Regex = settings.DataRules.OfType<RegexDataRule>()
                .Select(r => r.Adapt<RegexDataRuleDto>(config))
                .ToList()
            },
            Resources = new ResourcesDto
            {
                LinesFromFile = settings.Resources.OfType<LinesFromFileResourceOptions>()
                .Select(r => r.Adapt<LinesFromFileResourceDto>(config))
                .ToList(),
                RandomLinesFromFile = settings.Resources.OfType<RandomLinesFromFileResourceOptions>()
                .Select(r => r.Adapt<RandomLinesFromFileResourceDto>(config))
                .ToList()
            }
        };

    internal static DataSettings ToDataSettings(
        ConfigDataSettingsDto dto, TypeAdapterConfig config) => new()
        {
            AllowedWordlistTypes = dto.AllowedWordlistTypes,
            UrlEncodeDataAfterSlicing = dto.UrlEncodeDataAfterSlicing,
            DataRules =
        [
            .. dto.DataRules.Simple.Select(r => r.Adapt<SimpleDataRule>(config)),
            .. dto.DataRules.Regex.Select(r => r.Adapt<RegexDataRule>(config))
        ],
            Resources =
        [
            .. dto.Resources.LinesFromFile.Select(r => r.Adapt<LinesFromFileResourceOptions>(config)),
            .. dto.Resources.RandomLinesFromFile.Select(r => r.Adapt<RandomLinesFromFileResourceOptions>(config))
        ]
        };

    internal static TriggeredActionDto ToTriggeredActionDto(
        TriggeredAction action, TypeAdapterConfig config)
    {
        var dto = new TriggeredActionDto
        {
            Id = action.Id,
            Name = action.Name,
            IsActive = action.IsActive,
            IsRepeatable = action.IsRepeatable,
            JobId = action.JobId,
            Triggers = PolyMapper.MapAllFrom(action.Triggers, config),
            Actions = PolyMapper.MapAllFrom(action.Actions, config)
        };

        typeof(TriggeredActionDto)
            .GetProperty(nameof(TriggeredActionDto.Executions))!
            .SetValue(dto, action.Executions);

        return dto;
    }

    internal static TriggeredAction ToTriggeredAction(
        CreateTriggeredActionDto dto, TypeAdapterConfig config) => new()
        {
            Name = dto.Name,
            IsActive = dto.IsActive,
            IsRepeatable = dto.IsRepeatable,
            JobId = dto.JobId,
            Triggers = PolyMapper.MapBetween<TriggerDto, Trigger>(dto.Triggers, config),
            Actions = PolyMapper.MapBetween<ActionDto, Action>(dto.Actions, config)
        };

    internal static TriggeredAction ApplyTriggeredAction(
        UpdateTriggeredActionDto dto, TriggeredAction target, TypeAdapterConfig config)
    {
        target.IsActive = dto.IsActive;
        target.IsRepeatable = dto.IsRepeatable;
        target.JobId = dto.JobId;

        target.Triggers.Clear();
        target.Triggers.AddRange(PolyMapper.MapBetween<TriggerDto, Trigger>(dto.Triggers, config));

        target.Actions.Clear();
        target.Actions.AddRange(PolyMapper.MapBetween<ActionDto, Action>(dto.Actions, config));

        return new TriggeredAction
        {
            Id = target.Id,
            Name = dto.Name,
            IsActive = target.IsActive,
            IsExecuting = target.IsExecuting,
            IsRepeatable = target.IsRepeatable,
            Executions = target.Executions,
            JobId = target.JobId,
            Triggers = target.Triggers,
            Actions = target.Actions
        };
    }

    internal static TimeElapsedTrigger ToTimeElapsedTrigger(TimeElapsedTriggerDto dto) => new()
    {
        Comparison = dto.Comparison,
        Days = dto.TimeSpan.Days,
        Hours = dto.TimeSpan.Hours,
        Minutes = dto.TimeSpan.Minutes,
        Seconds = dto.TimeSpan.Seconds
    };

    internal static TimeRemainingTrigger ToTimeRemainingTrigger(TimeRemainingTriggerDto dto) => new()
    {
        Comparison = dto.Comparison,
        Days = dto.TimeSpan.Days,
        Hours = dto.TimeSpan.Hours,
        Minutes = dto.TimeSpan.Minutes,
        Seconds = dto.TimeSpan.Seconds
    };

    internal static TimeElapsedTriggerDto ToTimeElapsedTriggerDto(TimeElapsedTrigger trigger) => new()
    {
        Comparison = trigger.Comparison,
        TimeSpan = new TimeSpan(trigger.Days, trigger.Hours, trigger.Minutes, trigger.Seconds)
    };

    internal static TimeRemainingTriggerDto ToTimeRemainingTriggerDto(TimeRemainingTrigger trigger) => new()
    {
        Comparison = trigger.Comparison,
        TimeSpan = new TimeSpan(trigger.Days, trigger.Hours, trigger.Minutes, trigger.Seconds)
    };

    internal static WaitAction ToWaitAction(WaitActionDto dto) => new()
    {
        Days = dto.TimeSpan.Days,
        Hours = dto.TimeSpan.Hours,
        Minutes = dto.TimeSpan.Minutes,
        Seconds = dto.TimeSpan.Seconds
    };

    internal static WaitActionDto ToWaitActionDto(WaitAction action) => new()
    {
        TimeSpan = new TimeSpan(action.Days, action.Hours, action.Minutes, action.Seconds)
    };

    internal static SetRelativeStartConditionAction ToSetRelativeStartConditionAction(
        SetRelativeStartConditionActionDto dto) => new()
        {
            JobId = dto.JobId,
            Days = dto.TimeSpan.Days,
            Hours = dto.TimeSpan.Hours,
            Minutes = dto.TimeSpan.Minutes,
            Seconds = dto.TimeSpan.Seconds
        };

    internal static SetRelativeStartConditionActionDto ToSetRelativeStartConditionActionDto(
        SetRelativeStartConditionAction action) => new()
        {
            JobId = action.JobId,
            TimeSpan = new TimeSpan(action.Days, action.Hours, action.Minutes, action.Seconds)
        };

    internal static BlockInstanceDto ToBlockInstanceDto(BlockInstance block, TypeAdapterConfig config) =>
        block switch
        {
            AutoBlockInstance x => new AutoBlockInstanceDto
            {
                Id = x.Id,
                Disabled = x.Disabled,
                Label = x.Label,
                Settings = x.Settings.ToDictionary(kvp => kvp.Key, kvp => BlockSettingMapper.ToDto(kvp.Value)),
                Type = BlockInstanceType.Auto,
                OutputVariable = x.OutputVariable,
                IsCapture = x.IsCapture,
                Safe = x.Safe
            },
            ParseBlockInstance x => new ParseBlockInstanceDto
            {
                Id = x.Id,
                Disabled = x.Disabled,
                Label = x.Label,
                Settings = x.Settings.ToDictionary(kvp => kvp.Key, kvp => BlockSettingMapper.ToDto(kvp.Value)),
                Type = BlockInstanceType.Parse,
                OutputVariable = x.OutputVariable,
                Recursive = x.Recursive,
                IsCapture = x.IsCapture,
                Safe = x.Safe,
                Mode = x.Mode
            },
            ScriptBlockInstance x => new ScriptBlockInstanceDto
            {
                Id = x.Id,
                Disabled = x.Disabled,
                Label = x.Label,
                Settings = x.Settings.ToDictionary(kvp => kvp.Key, kvp => BlockSettingMapper.ToDto(kvp.Value)),
                Type = BlockInstanceType.Script,
                Script = x.Script,
                InputVariables = x.InputVariables,
                Interpreter = x.Interpreter,
                OutputVariables = x.OutputVariables
            },
            KeycheckBlockInstance x => new KeycheckBlockInstanceDto
            {
                Id = x.Id,
                Disabled = x.Disabled,
                Label = x.Label,
                Settings = x.Settings.ToDictionary(kvp => kvp.Key, kvp => BlockSettingMapper.ToDto(kvp.Value)),
                Type = BlockInstanceType.Keycheck,
                Keychains = x.Keychains.Select(k => ToKeychainDto(k, config)).ToList()
            },
            LoliCodeBlockInstance x => new LoliCodeBlockInstanceDto
            {
                Id = x.Id,
                Disabled = x.Disabled,
                Label = x.Label,
                Settings = x.Settings.ToDictionary(kvp => kvp.Key, kvp => BlockSettingMapper.ToDto(kvp.Value)),
                Type = BlockInstanceType.LoliCode,
                Script = x.Script
            },
            HttpRequestBlockInstance x => new HttpRequestBlockInstanceDto
            {
                Id = x.Id,
                Disabled = x.Disabled,
                Label = x.Label,
                Settings = x.Settings.ToDictionary(kvp => kvp.Key, kvp => BlockSettingMapper.ToDto(kvp.Value)),
                Type = BlockInstanceType.HttpRequest,
                Safe = x.Safe,
                RequestParams = PolyMapper.MapFrom(x.RequestParams, config)
            },
            _ => throw new NotImplementedException()
        };

    internal static KeychainDto ToKeychainDto(Keychain keychain, TypeAdapterConfig config) => new()
    {
        Mode = keychain.Mode,
        ResultStatus = keychain.ResultStatus,
        Keys = PolyMapper.MapAllFrom(keychain.Keys, config)
    };

    internal static MultipartRequestParamsDto ToMultipartRequestParamsDto(
        MultipartRequestParams requestParams, TypeAdapterConfig config) => new()
        {
            Boundary = BlockSettingMapper.ToDto(requestParams.Boundary),
            Contents = PolyMapper.MapAllFrom(requestParams.Contents, config)
        };

    internal static BlockDescriptorDto ToBlockDescriptorDto(
        BlockDescriptor descriptor, TypeAdapterConfig config) => new()
        {
            Id = descriptor.Id,
            Name = descriptor.Name,
            Description = descriptor.Description,
            ExtraInfo = descriptor.ExtraInfo,
            ReturnType = descriptor.ReturnType,
            Category = descriptor.Category.Adapt<BlockCategoryDto>(config),
            Parameters = descriptor.Parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => PolyMapper.MapFrom(kvp.Value, config))
        };

    internal static EnumBlockParameterDto ToEnumBlockParameterDto(EnumParameter parameter) => new()
    {
        Name = parameter.Name,
        AssignedName = parameter.AssignedName ?? string.Empty,
        Description = parameter.Description,
        InputMode = parameter.InputMode,
        DefaultVariableName = parameter.DefaultVariableName,
        Type = parameter.EnumType.Name,
        DefaultValue = parameter.DefaultValue,
        Options = parameter.Options
    };

    internal static void ApplyRuriLibSettings(
        RuriLib.Models.Settings.GlobalSettings source,
        RuriLib.Models.Settings.GlobalSettings destination,
        Interfaces.IObjectMapper mapper)
    {
        mapper.Map(source.GeneralSettings, destination.GeneralSettings);
        mapper.Map(source.CaptchaSettings, destination.CaptchaSettings);
        mapper.Map(source.ProxySettings, destination.ProxySettings);
        mapper.Map(source.PuppeteerSettings, destination.PuppeteerSettings);
        mapper.Map(source.SeleniumSettings, destination.SeleniumSettings);
    }

    internal static void ApplyOpenBulletSettings(
        OpenBulletSettingsDto source,
        OpenBulletSettings destination,
        Interfaces.IObjectMapper mapper)
    {
        mapper.Map(source.GeneralSettings, destination.GeneralSettings);
        mapper.Map(source.RemoteSettings, destination.RemoteSettings);
        mapper.Map(source.SecuritySettings, destination.SecuritySettings);
        mapper.Map(source.CustomizationSettings, destination.CustomizationSettings);
    }
}
