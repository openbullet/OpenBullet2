namespace OpenBullet2.Core.Options;

public class UserDataSettingsOptions
{
    public const string SectionName = "Settings";
    public const string DefaultUserDataFolder = "UserData";

    public string UserDataFolder { get; set; } = DefaultUserDataFolder;
}
