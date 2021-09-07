namespace OpenBullet2.Core.Models.Settings
{
    /// <summary>
    /// Settings for the OpenBullet 2 application.
    /// </summary>
    public class OpenBulletSettings
    {
        /// <summary>
        /// General settings.
        /// </summary>
        public GeneralSettings GeneralSettings { get; set; } = new GeneralSettings();

        /// <summary>
        /// Settings related to remote repositories.
        /// </summary>
        public RemoteSettings RemoteSettings { get; set; } = new RemoteSettings();

        /// <summary>
        /// Settings related to security.
        /// </summary>
        public SecuritySettings SecuritySettings { get; set; } = new SecuritySettings();

        /// <summary>
        /// Settings related to the appearance of the UI.
        /// </summary>
        public CustomizationSettings CustomizationSettings { get; set; } = new CustomizationSettings();
    }
}
