using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using RuriLib.Functions.Crypto;
using RuriLib.Models.Jobs.Monitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenBullet2.Core.Services;

/// <summary>
/// Monitors jobs, checks defined triggers every second and executes the corresponding actions.
/// </summary>
public class JobMonitorService : IDisposable
{
    /// <summary>
    /// The list of triggered actions that can be executed by the job monitor.
    /// </summary>
    public List<TriggeredAction> TriggeredActions { get; set; } = [];

    private readonly Timer timer;
    private readonly Timer? saveTimer;
    private readonly JobManagerService jobManager;
    private readonly TriggeredActionExecutor triggeredActionExecutor;
    private readonly ILogger<JobMonitorService> logger;
    private readonly string fileName;
    private readonly JsonSerializerSettings jsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented
    };
    private byte[] lastSavedHash = [];

    public JobMonitorService(JobManagerService jobManager,
        TriggeredActionExecutor triggeredActionExecutor,
        ILogger<JobMonitorService> logger,
        string fileName = "UserData/triggeredActions.json", bool autoSave = true)
    {
        this.jobManager = jobManager;
        this.triggeredActionExecutor = triggeredActionExecutor;
        this.logger = logger;
        this.fileName = fileName;
        RestoreTriggeredActions();

        timer = new Timer(new TimerCallback(_ => CheckAndExecute()), null, 1000, 1000);

        if (autoSave)
        {
            saveTimer = new Timer(new TimerCallback(_ => SaveStateIfChanged()), null, 5000, 5000);
        }
    }

    private void CheckAndExecute()
    {
        for (var i = 0; i < TriggeredActions.Count; i++)
        {
            var action = TriggeredActions[i];

            if (action.IsActive && !action.IsExecuting && (action.IsRepeatable || action.Executions == 0))
            {
                _ = triggeredActionExecutor.CheckAndExecuteAsync(action, jobManager.Jobs);
            }
        }
    }

    private void RestoreTriggeredActions()
    {
        if (!File.Exists(fileName))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(fileName);
            TriggeredActions = JsonConvert.DeserializeObject<TriggeredAction[]>(json, jsonSettings)?.ToList() ?? [];
        }
        catch
        {
            logger.LogWarning("Failed to deserialize triggered actions from {FileName}, recreating them", fileName);
        }
    }

    public void SaveStateIfChanged()
    {
        var json = JsonConvert.SerializeObject(TriggeredActions.ToArray(), jsonSettings);
        var hash = Crypto.MD5(Encoding.UTF8.GetBytes(json));

        if (hash != lastSavedHash)
        {
            try
            {
                File.WriteAllText(fileName, json);
                lastSavedHash = hash;
            }
            catch
            {
                logger.LogDebug("Could not save triggered actions to {FileName}, the file might be in use", fileName);
            }
        }
    }

    public void Dispose()
    {
        timer?.Dispose();
        saveTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
