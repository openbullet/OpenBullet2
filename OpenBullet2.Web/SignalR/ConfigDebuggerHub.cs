using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.ConfigDebugger;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Services;
using OpenBullet2.Web.Utils;
using RuriLib.Logging;
using RuriLib.Models.Debugger;
using RuriLib.Models.Variables;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for the config debugger.
/// </summary>
public class ConfigDebuggerHub : AuthorizedHub
{
    private readonly ConfigDebuggerService _debuggerService;
    private readonly ILogger<ConfigDebuggerHub> _logger;
    private readonly IMapper _mapper;
    private ConfigDebugger? _debugger;
    private string _configId = string.Empty;

    // Event handlers
    private readonly EventHandler<BotLoggerEntry> _onNewLog;
    private readonly EventHandler<ConfigDebuggerStatus> _onStatusChanged;

    /// <summary></summary>
    public ConfigDebuggerHub(ConfigDebuggerService debuggerService,
        IAuthTokenService tokenService, ILogger<ConfigDebuggerHub> logger,
        IMapper mapper)
        : base(tokenService, onlyAdmin: true)
    {
        _debuggerService = debuggerService;
        _logger = logger;
        _mapper = mapper;

        // Create the event handlers
        _onNewLog = EventHandlers.TryAsync<BotLoggerEntry>(
            OnNewLogEntry,
            BroadcastError
        );

        _onStatusChanged = EventHandlers.TryAsync<ConfigDebuggerStatus>(
            OnStatusChanged,
            BroadcastError
        );
    }

    /// <inheritdoc/>
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        // Read the id of the config that is being debugged
        var request = Context.GetHttpContext()!.Request;
        var configId = request.Query["configId"].FirstOrDefault();

        if (configId is null)
        {
            throw new Exception("Please specify a config id");
        }

        _configId = configId;

        // Try to get an existing debugger
        _debugger = _debuggerService.TryGet(_configId);

        // If there is none, just return
        if (_debugger is null)
        {
            return;
        }

        // Otherwise, hook the event listeners
        _debugger.NewLogEntry += _onNewLog;
        _debugger.StatusChanged += _onStatusChanged;
    }

    /// <summary>
    /// Start the debugger with the given options.
    /// </summary>
    public async Task Start(DbgStartRequestDto dto)
    {
        // We don't need to unsubscribe here since the publisher
        // will get GC'd
        _debugger = _debuggerService.CreateNew(
            _configId,
            new DebuggerOptions
            {
                PersistLog = dto.PersistLog
            }
        );

        // Hook the event listeners to the new debugger
        _debugger.NewLogEntry += _onNewLog;
        _debugger.StatusChanged += _onStatusChanged;

        // Fire and forget
        _ = Task.Run(async () =>
        {
            // Wrap everything inside a try/catch so we don't
            // lose the exceptions.
            try
            {
                await _debugger.Run();
            }
            catch (Exception ex)
            {
                var message = new DbgErrorMessage
                {
                    Type = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.ToString()
                };
                await Clients.All.SendAsync(Methods.Error, message);
            }
        });
    }

    /// <summary>
    /// Stop the debugger.
    /// </summary>
    public Task Stop()
    {
        if (_debugger is null)
        {
            throw new Exception("Start the debugger first");
        }

        if (_debugger.Status is ConfigDebuggerStatus.Idle)
        {
            throw new Exception("The debugger is not running");
        }

        _debugger.Stop();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Take a step in step-by-step mode.
    /// </summary>
    public Task TakeStep()
    {
        if (_debugger is null)
        {
            throw new Exception("Start the debugger first");
        }

        if (_debugger.Status is not ConfigDebuggerStatus.WaitingForStep)
        {
            throw new Exception("The debugger is not waiting for a step");
        }

        if (!_debugger.Options.StepByStep)
        {
            throw new Exception("The debugger is not in step by step mode");
        }

        _debugger.TryTakeStep();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get the state of the debugger.
    /// </summary>
    public async Task GetState()
    {
        DbgStateDto? state = null;

        // Try to get an existing debugger
        _debugger = _debuggerService.TryGet(_configId);

        // If there is a debugger
        if (_debugger is not null)
        {
            state = new DbgStateDto
            {
                Log = _debugger.Logger.Entries,
                Status = _debugger.Status,
                Variables = _debugger.Options.Variables.Select(MapVariable)
            };
        }

        await Clients.All.SendAsync(Methods.DebuggerState, state);
    }

    private async Task OnNewLogEntry(object? sender, BotLoggerEntry e)
    {
        var message = new DbgNewLogMessage
        {
            NewMessage = e
        };

        // Broadcast
        await Clients.All.SendAsync(Methods.NewLogEntry, message);
    }

    private async Task OnStatusChanged(object? sender, ConfigDebuggerStatus e)
    {
        var message = new DbgStatusChangedMessage
        {
            NewStatus = e
        };

        // Broadcast
        await Clients.All.SendAsync(Methods.StatusChanged, message);

        // Right now, only when the status goes back to idle, we
        // update the variables.
        // TODO: In the future it would be nice to update them more often.
        var varMessage = new DbgVariablesChangedMessage
        {
            Variables = (sender as ConfigDebugger)!.Options.Variables.Select(MapVariable)
        };

        // Broadcast
        await Clients.All.SendAsync(Methods.VariablesChanged, varMessage);
    }

    private void BroadcastError(Exception ex)
        => _logger.LogError(ex, "Error while broadcasting message to clients");

    private static VariableDto MapVariable(Variable v)
        => new()
        {
            Name = v.Name,
            MarkedForCapture = v.MarkedForCapture,
            Type = v.Type,
            Value = v switch
            {
                StringVariable x => x.AsString(),
                IntVariable x => x.AsInt(),
                FloatVariable x => x.AsFloat(),
                ListOfStringsVariable x => x.AsListOfStrings(),
                DictionaryOfStringsVariable x => x.AsDictionaryOfStrings(),
                BoolVariable x => x.AsBool(),
                ByteArrayVariable x => x.AsByteArray(),
                _ => throw new NotImplementedException()
            }
        };

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_debugger is not null)
        {
            _debugger.NewLogEntry -= _onNewLog;
            _debugger.StatusChanged -= _onStatusChanged;
        }

        base.Dispose(disposing);
    }

    private class Methods
    {
        public const string NewLogEntry = "newLogEntry";
        public const string DebuggerState = "debuggerState";
        public const string StatusChanged = "statusChanged";
        public const string VariablesChanged = "variablesChanged";
        public const string Error = "error";
    }
}
