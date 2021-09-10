namespace RuriLib.Models.Configs.Settings
{
    public class GeneralSettings
    {
        public int SuggestedBots { get; set; } = 1;
        public int MaximumCPM { get; set; } = 0;
        public bool SaveEmptyCaptures { get; set; } = false;
        public bool ReportLastCaptchaOnRetry { get; set; } = false;
        
        public string[] ContinueStatuses { get; set; } = new string[]
        {
            "SUCCESS",
            "NONE"
        };
    }
}
