using System;

namespace OpenBullet2.Web.Updater.Models;

public record RemoteVersionInfo(Version Version, string DownloadUrl, double Size);
