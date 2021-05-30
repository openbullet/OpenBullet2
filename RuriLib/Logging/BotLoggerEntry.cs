namespace RuriLib.Logging
{
    /// <summary>
    /// An entry of a <see cref="BotLogger"/>.
    /// </summary>
    public class BotLoggerEntry
    {
        /// <summary>
        /// The logged message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The color of the message when displayed in a UI.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Whether the message contains HTML code and can be rendered as HTML.
        /// </summary>
        public bool CanViewAsHtml { get; set; } = false;
    }
}
