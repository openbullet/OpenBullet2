using System;

namespace OpenBullet2.Native.Updater.Models;

public record RemoteVersionInfo(Version Version, string DownloadUrl, double Size);
