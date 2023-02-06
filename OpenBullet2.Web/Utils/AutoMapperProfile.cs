using AutoMapper;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Settings;
using OpenBullet2.Core.Models.Sharing;
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
using System.Reflection;
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
                (s, d, i, ctx) => MapTriggers(s.Triggers, ctx.Mapper)));

        RegisterTriggerMaps();
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

    // TODO: Find a better way to get automapper to do this automatically...
    private static List<object> MapTriggers(IEnumerable<Trigger> triggers,
        IRuntimeMapper mapper)
    {
        var mappedList = new List<object>();

        foreach (var trigger in triggers)
        {
            TriggerDto mapped = trigger switch
            {
                JobStatusTrigger x => mapper.Map<JobStatusTriggerDto>(x),
                JobFinishedTrigger x => mapper.Map<JobFinishedTriggerDto>(x),
                TestedCountTrigger x => mapper.Map<TestedCountTriggerDto>(x),
                HitCountTrigger x => mapper.Map<HitCountTriggerDto>(x),
                CustomCountTrigger x => mapper.Map<CustomCountTriggerDto>(x),
                ToCheckCountTrigger x => mapper.Map<ToCheckCountTriggerDto>(x),
                FailCountTrigger x => mapper.Map<FailCountTriggerDto>(x),
                RetryCountTrigger x => mapper.Map<RetryCountTriggerDto>(x),
                BanCountTrigger x => mapper.Map<BanCountTriggerDto>(x),
                ErrorCountTrigger x => mapper.Map<ErrorCountTriggerDto>(x),
                AliveProxiesCountTrigger x => mapper.Map<AliveProxiesCountTriggerDto>(x),
                BannedProxiesCountTrigger x => mapper.Map<BannedProxiesCountTriggerDto>(x),
                CPMTrigger x => mapper.Map<CPMTriggerDto>(x),
                CaptchaCreditTrigger x => mapper.Map<CaptchaCreditTriggerDto>(x),
                ProgressTrigger x => mapper.Map<ProgressTriggerDto>(x),
                TimeElapsedTrigger x => mapper.Map<TimeElapsedTriggerDto>(x),
                TimeRemainingTrigger x => mapper.Map<TimeRemainingTriggerDto>(x),
                _ => throw new NotImplementedException()
            };

            mapped.PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType(
                mapped.GetType()) ?? string.Empty;

            mappedList.Add(mapped);
        }

        return mappedList;
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

    private static List<T> ConvertPolyDtoList<T>(
        IEnumerable<JsonDocument>? list) where T : PolyDto
    {
        if (list is null)
        {
            return new List<T>();
        }

        var subTypes = PolyDtoCache.GetSubTypes<T>();

        if (subTypes.Length == 0)
        {
            throw new Exception($"No subtypes found for type {typeof(T).FullName}");
        }

        var items = new List<T>();

        foreach (var jsonDocument in list)
        {
            var item = ConvertPolyDto<T>(jsonDocument);

            if (item is not null)
            {
                items.Add(item);
            }
        }

        return items;
    }

    private static T? ConvertPolyDto<T>(
        JsonDocument? jsonDocument) where T : PolyDto
    {
        if (jsonDocument is null)
        {
            return null;
        }

        var polyTypeName = jsonDocument.RootElement
            .GetProperty("_polyTypeName").GetString();

        if (polyTypeName is null)
        {
            throw new Exception($"The json document has no _polyTypeName field");
        }

        var subType = PolyDtoCache.GetPolyTypeFromName(polyTypeName);

        if (subType is null)
        {
            var validTypeNames = PolyDtoCache.GetValidPolyTypeNames<T>();
            throw new Exception($"Invalid _polyTypeName: {polyTypeName}. Valid values: {string.Join(", ", validTypeNames)}");
        }

        return (T?)jsonDocument.Deserialize(subType);
    }

    private static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(T), false).Length > 0)
            {
                yield return type;
            }
        }
    }
}
