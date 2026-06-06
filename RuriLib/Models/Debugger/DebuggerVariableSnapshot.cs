using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Debugger;

/// <summary>
/// A typed variable snapshot captured while a config is paused in the debugger.
/// </summary>
public sealed class DebuggerVariableSnapshotEntry
{
    /// <summary>
    /// The variable name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The variable type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The variable value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Creates a new snapshot entry.
    /// </summary>
    public DebuggerVariableSnapshotEntry(string name, Type type, object? value)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("The variable name cannot be null or empty", nameof(name))
            : name;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Value = value;
    }

    /// <summary>
    /// Creates a new snapshot entry by inferring the type from the generic argument.
    /// </summary>
    public static DebuggerVariableSnapshotEntry Create<T>(string name, T value)
        => new(name, typeof(T), value);
}

/// <summary>
/// Stores and retrieves debugger variable snapshots from a bot context.
/// </summary>
public static class DebuggerVariableSnapshot
{
    private const string ObjectKey = "debuggerVariableSnapshot";

    /// <summary>
    /// Stores the latest variable snapshot for a bot.
    /// </summary>
    public static void Store(BotData data, IEnumerable<DebuggerVariableSnapshotEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(entries);

        var snapshot = GetOrCreateEntries(data);
        snapshot.Clear();

        foreach (var entry in entries)
        {
            snapshot[entry.Name] = entry;
        }
    }

    /// <summary>
    /// Stores or replaces a single debugger-visible variable.
    /// </summary>
    public static void Set<T>(BotData data, string name, T value)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        GetOrCreateEntries(data)[name] = DebuggerVariableSnapshotEntry.Create(name, value);
    }

    /// <summary>
    /// Clears all stored debugger-visible variables for a bot.
    /// </summary>
    public static void Clear(BotData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        GetOrCreateEntries(data).Clear();
    }

    /// <summary>
    /// Gets the latest variable snapshot for a bot.
    /// </summary>
    public static IReadOnlyList<DebuggerVariableSnapshotEntry> Get(BotData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        return GetOrCreateEntries(data).Values.ToList();
    }

    private static Dictionary<string, DebuggerVariableSnapshotEntry> GetOrCreateEntries(BotData data)
    {
        var snapshot = data.TryGetObject<Dictionary<string, DebuggerVariableSnapshotEntry>>(ObjectKey);

        if (snapshot is not null)
        {
            return snapshot;
        }

        snapshot = new Dictionary<string, DebuggerVariableSnapshotEntry>(StringComparer.Ordinal);
        data.SetObject(ObjectKey, snapshot, disposeExisting: false);
        return snapshot;
    }
}
