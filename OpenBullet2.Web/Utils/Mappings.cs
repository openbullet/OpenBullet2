using AutoMapper;
using OpenBullet2.Web.Dtos;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Extensions;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenBullet2.Web.Utils;

static internal class Mappings
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private static readonly Dictionary<Type, Type> _triggers = new Dictionary<Type, Type>()
    {
        [typeof(JobStatusTrigger)] = typeof(JobStatusTriggerDto),
        [typeof(JobFinishedTrigger)] = typeof(JobFinishedTriggerDto),
        [typeof(TestedCountTrigger)] = typeof(TestedCountTriggerDto),
        [typeof(HitCountTrigger)] = typeof(HitCountTriggerDto),
        [typeof(CustomCountTrigger)] = typeof(CustomCountTriggerDto),
        [typeof(ToCheckCountTrigger)] = typeof(ToCheckCountTriggerDto),
        [typeof(FailCountTrigger)] = typeof(FailCountTriggerDto),
        [typeof(RetryCountTrigger)] = typeof(RetryCountTriggerDto),
        [typeof(BanCountTrigger)] = typeof(BanCountTriggerDto),
        [typeof(ErrorCountTrigger)] = typeof(ErrorCountTriggerDto),
        [typeof(AliveProxiesCountTrigger)] = typeof(AliveProxiesCountTriggerDto),
        [typeof(BannedProxiesCountTrigger)] = typeof(BannedProxiesCountTriggerDto),
        [typeof(CPMTrigger)] = typeof(CPMTriggerDto),
        [typeof(CaptchaCreditTrigger)] = typeof(CaptchaCreditTriggerDto),
        [typeof(ProgressTrigger)] = typeof(ProgressTriggerDto),
        [typeof(TimeElapsedTrigger)] = typeof(TimeElapsedTriggerDto),
        [typeof(TimeRemainingTrigger)] = typeof(TimeRemainingTriggerDto)
    }.MapReverse();

    private static readonly Dictionary<Type, Type> _actions = new Dictionary<Type, Type>
    {
        [typeof(WaitAction)] = typeof(WaitActionDto),
        [typeof(SetRelativeStartConditionAction)] = typeof(SetRelativeStartConditionActionDto),
        [typeof(StopJobAction)] = typeof(StopJobActionDto),
        [typeof(AbortJobAction)] = typeof(AbortJobActionDto),
        [typeof(StartJobAction)] = typeof(StartJobActionDto),
        [typeof(DiscordWebhookAction)] = typeof(DiscordWebhookActionDto),
        [typeof(TelegramBotAction)] = typeof(TelegramBotActionDto),
        [typeof(SetBotsAction)] = typeof(SetBotsActionDto),
        [typeof(ReloadProxiesAction)] = typeof(ReloadProxiesActionDto)
    }.MapReverse();

    static internal List<object> MapTriggers(IEnumerable<Trigger> triggers,
        IRuntimeMapper mapper)
    {
        var mappedList = new List<object>();

        foreach (var trigger in triggers)
        {
            var mappedType = _triggers[trigger.GetType()];
            var mapped = (PolyDto)mapper.Map(trigger, trigger.GetType(), mappedType);

            mapped.PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType(
                mapped.GetType()) ?? string.Empty;

            mappedList.Add(mapped);
        }

        return mappedList;
    }

    static internal List<Trigger> MapTriggers(
        IEnumerable<JsonDocument> jsonDocuments,
        IRuntimeMapper mapper)
    {
        var mappedList = new List<Trigger>();
        var triggers = ConvertPolyDtoList<TriggerDto>(jsonDocuments);

        foreach (var trigger in triggers)
        {
            var type = trigger.GetType();
            var targetType = _triggers[type];
            var mapped = (Trigger)mapper.Map(trigger, type, targetType);

            mappedList.Add(mapped);
        }

        return mappedList;
    }

    static internal List<object> MapActions(
        IEnumerable<RuriLib.Models.Jobs.Monitor.Actions.Action> actions,
        IRuntimeMapper mapper)
    {
        var mappedList = new List<object>();

        foreach (var action in actions)
        {
            var mappedType = _actions[action.GetType()];
            var mapped = (PolyDto)mapper.Map(action, action.GetType(), mappedType);

            mapped.PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType(
                mapped.GetType()) ?? string.Empty;

            mappedList.Add(mapped);
        }

        return mappedList;
    }

    static internal List<RuriLib.Models.Jobs.Monitor.Actions.Action> MapActions(
        IEnumerable<JsonDocument> jsonDocuments,
        IRuntimeMapper mapper)
    {
        var mappedList = new List<RuriLib.Models.Jobs.Monitor.Actions.Action>();
        var actions = ConvertPolyDtoList<ActionDto>(jsonDocuments);

        foreach (var action in actions)
        {
            var type = action.GetType();
            var targetType = _actions[type];
            var mapped = (RuriLib.Models.Jobs.Monitor.Actions.Action)mapper.Map(action, type, targetType);

            mappedList.Add(mapped);
        }

        return mappedList;
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

        return (T?)jsonDocument.Deserialize(subType, _jsonOptions);
    }
}
