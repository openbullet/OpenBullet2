namespace OpenBullet2.Core.Models.Settings
{
    /// <summary>
    /// Settings related to the appearance of the OpenBullet2 GUI.
    /// </summary>
    public class CustomizationSettings
    {
        /// <summary>
        /// The theme to use. Themes are included in separate files and identified
        /// by their name. Web UI only.
        /// </summary>
        public string Theme { get; set; } = "Default";

        /// <summary>
        /// The theme to use for the Monaco editor. Web UI only.
        /// </summary>
        public string MonacoTheme { get; set; } = "vs-dark";

        /// <summary>
        /// Whether to wrap words at viewport width.
        /// </summary>
        public bool WordWrap { get; set; } = false;

        /// <summary>
        /// The main background color. Native UI only.
        /// </summary>
        public string BackgroundMain { get; set; } = "#222";

        /// <summary>
        /// The background color for inputs. Native UI only.
        /// </summary>
        public string BackgroundInput { get; set; } = "#282828";

        /// <summary>
        /// The secondary background color. Native UI only.
        /// </summary>
        public string BackgroundSecondary { get; set; } = "#111";

        /// <summary>
        /// The main foreground color. Native UI only.
        /// </summary>
        public string ForegroundMain { get; set; } = "#DCDCDC";

        /// <summary>
        /// The foreground color for inputs. Native UI only.
        /// </summary>
        public string ForegroundInput { get; set; } = "#DCDCDC";

        /// <summary>
        /// The foreground color for hits. Native UI only.
        /// </summary>
        public string ForegroundGood { get; set; } = "#ADFF2F";

        /// <summary>
        /// The foreground color for fails. Native UI only.
        /// </summary>
        public string ForegroundBad { get; set; } = "#FF6347";

        /// <summary>
        /// The foreground color for custom hits. Native UI only.
        /// </summary>
        public string ForegroundCustom { get; set; } = "#FF8C00";

        /// <summary>
        /// The foreground color for retries. Native UI only.
        /// </summary>
        public string ForegroundRetry { get; set; } = "#FFFF00";

        /// <summary>
        /// The foreground color for bans. Native UI only.
        /// </summary>
        public string ForegroundBanned { get; set; } = "#DDA0DD";

        /// <summary>
        /// The foreground color for hits to check. Native UI only.
        /// </summary>
        public string ForegroundToCheck { get; set; } = "#7FFFD4";

        /// <summary>
        /// The foreground color for selected menu items. Native UI only.
        /// </summary>
        public string ForegroundMenuSelected { get; set; } = "#1E90FF";

        /// <summary>
        /// The color of success buttons. Native UI only.
        /// </summary>
        public string SuccessButton { get; set; } = "#2f5738";

        /// <summary>
        /// The color of primary buttons. Native UI only.
        /// </summary>
        public string PrimaryButton { get; set; } = "#3b3a63";

        /// <summary>
        /// The color of warning buttons. Native UI only.
        /// </summary>
        public string WarningButton { get; set; } = "#7a552a";

        /// <summary>
        /// The color of danger buttons. Native UI only.
        /// </summary>
        public string DangerButton { get; set; } = "#693838";

        /// <summary>
        /// The foreground color of buttons. Native UI only.
        /// </summary>
        public string ForegroundButton { get; set; } = "#DCDCDC";

        /// <summary>
        /// The background color of buttons. Native UI only.
        /// </summary>
        public string BackgroundButton { get; set; } = "#282828";

        /// <summary>
        /// The path to the background image. Native UI only.
        /// </summary>
        public string BackgroundImagePath { get; set; } = "";

        /// <summary>
        /// The opacity of the background image (from 0 to 100). Native UI only.
        /// </summary>
        public double BackgroundOpacity { get; set; } = 100;

        /// <summary>
        /// Whether to play a sound when a hit is found.
        /// </summary>
        public bool PlaySoundOnHit { get; set; } = false;
    }
}
