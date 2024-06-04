using System;

namespace Updater.Native.Models;

public record RemoteVersionInfo(Version Version, string DownloadUrl, double Size);
