using System;

namespace Updater.Models;

public record RemoteVersionInfo(Version Version, string DownloadUrl, double Size);
