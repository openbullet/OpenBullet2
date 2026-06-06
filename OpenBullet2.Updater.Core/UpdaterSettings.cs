namespace OpenBullet2.Updater.Core;

public record UpdaterSettings(
    string AssetName,
    Func<Task> EnsureNotRunningAsync);
