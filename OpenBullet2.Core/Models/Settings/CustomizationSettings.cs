namespace OpenBullet2.Core.Models.Settings
{
    /// <summary>
    /// Settings related to the appearance of the OpenBullet2 GUI.
    /// </summary>
    public class CustomizationSettings
    {
        /// <summary>
        /// The theme to use. Themes are included in separate files and identified
        /// by their name.
        /// </summary>
        public string Theme { get; set; } = "Default";

        /// <summary>
        /// The theme to use for the Monaco editor.
        /// </summary>
        public string MonacoTheme { get; set; } = "vs-dark";

        /// <summary>
        /// Whether to play a sound when a hit is found.
        /// </summary>
        public bool PlaySoundOnHit { get; set; } = false;
    }
}
