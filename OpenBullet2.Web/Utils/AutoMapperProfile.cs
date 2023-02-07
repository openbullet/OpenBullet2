using AutoMapper;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Web.Dtos;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Dtos.ProxyGroup;
using OpenBullet2.Web.Dtos.Settings;
using OpenBullet2.Web.Dtos.Shared;
using OpenBullet2.Web.Dtos.User;
using OpenBullet2.Web.Dtos.Wordlist;
using OpenBullet2.Web.Models.Pagination;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Environment;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.Text.Json;

namespace OpenBullet2.Web.Utils;

internal class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<CreateGuestDto, GuestEntity>()
            .ForMember(entity => entity.PasswordHash, e => e.MapFrom(dto => 
                BCrypt.Net.BCrypt.HashPassword(dto.Password, BCrypt.Net.SaltRevision.Revision2B)))
            .ForMember(entity => entity.AllowedAddresses, e => e.MapFrom(dto =>
                string.Join(',', dto.AllowedAddresses)));

        CreateMap<UpdateGuestInfoDto, GuestEntity>()
            .ForMember(entity => entity.AllowedAddresses, e => e.MapFrom(dto =>
                string.Join(',', dto.AllowedAddresses)));

        CreateMap<UpdateGuestPasswordDto, GuestEntity>()
            .ForMember(entity => entity.PasswordHash, e => e.MapFrom(dto =>
                BCrypt.Net.BCrypt.HashPassword(dto.Password, BCrypt.Net.SaltRevision.Revision2B)));

        CreateMap<GuestEntity, GuestDto>()
            .ForMember(dto => dto.AllowedAddresses, e => e.MapFrom(entity =>
                entity.AllowedAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));

        CreateMap<GuestEntity, OwnerDto>();

        CreateMap<EnvironmentSettings, EnvironmentSettingsDto>();
        CreateMap<OpenBulletSettings, OpenBulletSettingsDto>().ReverseMap();
        CreateMap<SecuritySettings, OBSecuritySettingsDto>().ReverseMap();
        CreateMap<Core.Models.Settings.GeneralSettings, OBGeneralSettingsDto>().ReverseMap();
        CreateMap<CustomizationSettings, OBCustomizationSettingsDto>().ReverseMap();

        CreateMap<HitEntity, HitDto>();
        CreateMap<UpdateHitDto, HitEntity>();

        CreateMap<CreateHitDto, HitEntity>()
            .ForMember(entity => entity.Proxy, e => e.MapFrom(dto => dto.Proxy ?? string.Empty))
            .ForMember(entity => entity.Date, e => e.MapFrom(dto => dto.Date ?? DateTime.Now))
            .ForMember(entity => entity.ConfigName, e => e.MapFrom(dto => dto.ConfigName ?? string.Empty))
            .ForMember(entity => entity.ConfigCategory, e => e.MapFrom(dto => dto.ConfigCategory ?? string.Empty))
            .ForMember(entity => entity.WordlistName, e => e.MapFrom(dto => dto.WordlistName ?? string.Empty));

        CreateMap<WordlistEntity, WordlistDto>()
            .ForMember(dto => dto.FilePath, e => e.MapFrom(entity => entity.FileName))
            .ForMember(dto => dto.LineCount, e => e.MapFrom(entity => entity.Total))
            .ForMember(dto => dto.WordlistType, e => e.MapFrom(entity => entity.Type))
            .ForMember(dto => dto.Owner, e => e.MapFrom(entity => entity.Owner));

        CreateMap<CreateWordlistDto, WordlistEntity>()
            .ForMember(entity => entity.Type, e => e.MapFrom(dto => dto.WordlistType))
            .ForMember(entity => entity.FileName, e => e.MapFrom(
                dto => dto.FilePath.Replace('\\', '/')))
            .ForMember(entity => entity.Total, e => e.MapFrom(
                dto => File.ReadLines(dto.FilePath).Count()));

        CreateMap<UpdateWordlistInfoDto, WordlistEntity>()
            .ForMember(entity => entity.Type, e => e.MapFrom(dto => dto.WordlistType));

        CreateMap<ProxyEntity, ProxyDto>()
            .ForMember(dto => dto.GroupId, e => e.MapFrom(entity => entity.Group.Id))
            .ForMember(dto => dto.LastChecked, e => e.MapFrom(entity =>
                entity.LastChecked == default ? null : (DateTime?)entity.LastChecked));

        CreateMap<ProxyGroupEntity, ProxyGroupDto>();
        CreateMap<CreateProxyGroupDto, ProxyGroupEntity>();
        CreateMap<UpdateProxyGroupDto, ProxyGroupEntity>();

        CreateMap<Config, ConfigInfoDto>()
            .ForMember(dto => dto.AllowedWordlistTypes, e => e.MapFrom(c =>
                c.Settings.DataSettings.AllowedWordlistTypes.ToList()))
            .ForMember(dto => dto.Base64Image, e => e.MapFrom(c => c.Metadata.Base64Image))
            .ForMember(dto => dto.Author, e => e.MapFrom(c => c.Metadata.Author))
            .ForMember(dto => dto.Category, e => e.MapFrom(c => c.Metadata.Category))
            .ForMember(dto => dto.CreationDate, e => e.MapFrom(c => c.Metadata.CreationDate))
            .ForMember(dto => dto.LastModified, e => e.MapFrom(c => c.Metadata.LastModified))
            .ForMember(dto => dto.Name, e => e.MapFrom(c => c.Metadata.Name))
            .ForMember(dto => dto.NeedsProxies, e => e.MapFrom(c => 
                c.Settings.ProxySettings.UseProxies));

        CreateMap<Config, ConfigDto>();
        CreateMap<ConfigMetadata, ConfigMetadataDto>().ReverseMap();
        CreateMap<ConfigSettings, ConfigSettingsDto>().ReverseMap();
        CreateMap<Core.Models.Settings.GeneralSettings, ConfigGeneralSettingsDto>().ReverseMap();
        CreateMap<ProxySettings, ConfigProxySettingsDto>().ReverseMap();
        CreateMap<InputSettings, ConfigInputSettingsDto>().ReverseMap();
        CreateMap<DataSettings, ConfigDataSettingsDto>()
            .ForMember(dto => dto.DataRules, e => e.MapFrom(
                (s, d, i, ctx) => new DataRulesDto
                {
                    Simple = ctx.Mapper.Map<List<SimpleDataRuleDto>>(
                        s.DataRules.OfType<SimpleDataRule>().ToList()),
                    Regex = ctx.Mapper.Map<List<RegexDataRuleDto>>(
                        s.DataRules.OfType<RegexDataRule>().ToList())
                }))
            .ForMember(dto => dto.Resources, e => e.MapFrom(
                (s, d, i, ctx) => new ResourcesDto
                {
                    LinesFromFile = ctx.Mapper.Map<List<LinesFromFileResourceDto>>(
                        s.DataRules.OfType<LinesFromFileResourceOptions>().ToList()),
                    RandomLinesFromFile = ctx.Mapper.Map<List<RandomLinesFromFileResourceDto>>(
                        s.DataRules.OfType<RandomLinesFromFileResourceOptions>().ToList())
                }))
            ;
        CreateMap<ConfigDataSettingsDto, DataSettings>()
            .ForMember(settings => settings.DataRules, e => e.MapFrom(
                    (s, d, i, ctx) => MapDataRules(s.DataRules, ctx.Mapper)
                ))
            .ForMember(settings => settings.Resources, e => e.MapFrom(
                    (s, d, i, ctx) => MapResources(s.Resources, ctx.Mapper)
                ));
        CreateMap<BrowserSettings, ConfigBrowserSettingsDto>().ReverseMap();
        CreateMap<ScriptSettings, ConfigScriptSettingsDto>().ReverseMap();
        CreateMap<CustomInput, CustomInputDto>().ReverseMap();
        CreateMap<SimpleDataRule, SimpleDataRuleDto>().ReverseMap();
        CreateMap<RegexDataRule, RegexDataRuleDto>().ReverseMap();
        CreateMap<LinesFromFileResourceOptions, LinesFromFileResourceDto>().ReverseMap();
        CreateMap<RandomLinesFromFileResourceOptions, RandomLinesFromFileResourceDto>().ReverseMap();

        CreateMap<Core.Models.Sharing.Endpoint, EndpointDto>().ReverseMap();

        // Allow conversion between PagedLists with different generic type
        // (the types must be mapped separately)
        CreateMap(typeof(PagedList<>), typeof(PagedList<>));

        CreateMap<TriggeredAction, TriggeredActionDto>()
            .ForMember(dto => dto.Triggers, e => e.MapFrom(
                (s, d, i, ctx) => Mappings.MapTriggers(s.Triggers, ctx.Mapper)))
            .ForMember(dto => dto.Actions, e => e.MapFrom(
                (s, d, i, ctx) => Mappings.MapActions(s.Actions, ctx.Mapper)));

        CreateMap<CreateTriggeredActionDto, TriggeredAction>()
            .ForMember(dto => dto.Triggers, e => e.MapFrom(
                (s, d, i, ctx) => Mappings.MapTriggers(s.Triggers, ctx.Mapper)))
            .ForMember(dto => dto.Actions, e => e.MapFrom(
                (s, d, i, ctx) => Mappings.MapActions(s.Actions, ctx.Mapper)));

        CreateMap<UpdateTriggeredActionDto, TriggeredAction>()
            .ForMember(dto => dto.Triggers, e => e.MapFrom(
                (s, d, i, ctx) => Mappings.MapTriggers(s.Triggers, ctx.Mapper)))
            .ForMember(dto => dto.Actions, e => e.MapFrom(
                (s, d, i, ctx) => Mappings.MapActions(s.Actions, ctx.Mapper)));

        RegisterTriggerMaps();
        RegisterActionMaps();
    }

    private static List<DataRule> MapDataRules(
        DataRulesDto dto, IRuntimeMapper mapper)
    {
        List<DataRule> rules = new();
        rules.AddRange(mapper.Map<List<SimpleDataRule>>(dto.Simple));
        rules.AddRange(mapper.Map<List<RegexDataRule>>(dto.Regex));
        return rules;
    }

    private static List<ConfigResourceOptions> MapResources(
        ResourcesDto dto, IRuntimeMapper mapper)
    {
        List<ConfigResourceOptions> resources = new();
        resources.AddRange(mapper.Map<List<LinesFromFileResourceOptions>>(
            dto.LinesFromFile));
        resources.AddRange(mapper.Map<List<RandomLinesFromFileResourceOptions>>(
            dto.RandomLinesFromFile));
        return resources;
    }

    private void RegisterTriggerMaps()
    {
        CreateMap<JobStatusTriggerDto, JobStatusTrigger>().ReverseMap();
        CreateMap<JobFinishedTriggerDto, JobFinishedTrigger>().ReverseMap();

        CreateMap<TestedCountTriggerDto, TestedCountTrigger>().ReverseMap();
        CreateMap<HitCountTriggerDto, HitCountTrigger>().ReverseMap();
        CreateMap<CustomCountTriggerDto, CustomCountTrigger>().ReverseMap();
        CreateMap<ToCheckCountTriggerDto, ToCheckCountTrigger>().ReverseMap();
        CreateMap<FailCountTriggerDto, FailCountTrigger>().ReverseMap();
        CreateMap<RetryCountTriggerDto, RetryCountTrigger>().ReverseMap();
        CreateMap<BanCountTriggerDto, BanCountTrigger>().ReverseMap();
        CreateMap<ErrorCountTriggerDto, ErrorCountTrigger>().ReverseMap();
        CreateMap<AliveProxiesCountTriggerDto, AliveProxiesCountTrigger>().ReverseMap();
        CreateMap<BannedProxiesCountTriggerDto, BannedProxiesCountTrigger>().ReverseMap();
        CreateMap<CPMTriggerDto, CPMTrigger>().ReverseMap();
        CreateMap<CaptchaCreditTriggerDto, CaptchaCreditTrigger>().ReverseMap();
        CreateMap<ProgressTriggerDto, ProgressTrigger>().ReverseMap();
        
        CreateMap<TimeElapsedTriggerDto, TimeElapsedTrigger>()
            .ForMember(t => t.Days, e => e.MapFrom(dto => dto.TimeSpan.Days))
            .ForMember(t => t.Hours, e => e.MapFrom(dto => dto.TimeSpan.Hours))
            .ForMember(t => t.Minutes, e => e.MapFrom(dto => dto.TimeSpan.Minutes))
            .ForMember(t => t.Seconds, e => e.MapFrom(dto => dto.TimeSpan.Seconds));

        CreateMap<TimeRemainingTriggerDto, TimeRemainingTrigger>()
            .ForMember(t => t.Days, e => e.MapFrom(dto => dto.TimeSpan.Days))
            .ForMember(t => t.Hours, e => e.MapFrom(dto => dto.TimeSpan.Hours))
            .ForMember(t => t.Minutes, e => e.MapFrom(dto => dto.TimeSpan.Minutes))
            .ForMember(t => t.Seconds, e => e.MapFrom(dto => dto.TimeSpan.Seconds));

        CreateMap<TimeElapsedTrigger, TimeElapsedTriggerDto>()
            .ForMember(dto => dto.TimeSpan, e => e.MapFrom(t =>
                new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds)));

        CreateMap<TimeRemainingTrigger, TimeRemainingTriggerDto>()
            .ForMember(dto => dto.TimeSpan, e => e.MapFrom(t =>
                new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds)));
    }

    private void RegisterActionMaps()
    {
        CreateMap<WaitActionDto, WaitAction>()
            .ForMember(t => t.Days, e => e.MapFrom(dto => dto.TimeSpan.Days))
            .ForMember(t => t.Hours, e => e.MapFrom(dto => dto.TimeSpan.Hours))
            .ForMember(t => t.Minutes, e => e.MapFrom(dto => dto.TimeSpan.Minutes))
            .ForMember(t => t.Seconds, e => e.MapFrom(dto => dto.TimeSpan.Seconds));

        CreateMap<WaitAction, WaitActionDto>()
            .ForMember(dto => dto.TimeSpan, e => e.MapFrom(t =>
                new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds)));

        CreateMap<SetRelativeStartConditionActionDto, SetRelativeStartConditionAction>()
            .ForMember(t => t.Days, e => e.MapFrom(dto => dto.TimeSpan.Days))
            .ForMember(t => t.Hours, e => e.MapFrom(dto => dto.TimeSpan.Hours))
            .ForMember(t => t.Minutes, e => e.MapFrom(dto => dto.TimeSpan.Minutes))
            .ForMember(t => t.Seconds, e => e.MapFrom(dto => dto.TimeSpan.Seconds));

        CreateMap<SetRelativeStartConditionAction, SetRelativeStartConditionActionDto>()
            .ForMember(dto => dto.TimeSpan, e => e.MapFrom(t =>
                new TimeSpan(t.Days, t.Hours, t.Minutes, t.Seconds)));

        CreateMap<StopJobAction, StopJobActionDto>().ReverseMap();
        CreateMap<AbortJobAction, AbortJobActionDto>().ReverseMap();
        CreateMap<StartJobAction, StartJobActionDto>().ReverseMap();
        CreateMap<DiscordWebhookAction, DiscordWebhookActionDto>().ReverseMap();
        CreateMap<TelegramBotAction, TelegramBotActionDto>().ReverseMap();
        CreateMap<SetBotsAction, SetBotsActionDto>().ReverseMap();
        CreateMap<ReloadProxiesAction, ReloadProxiesActionDto>().ReverseMap();
    }
}
