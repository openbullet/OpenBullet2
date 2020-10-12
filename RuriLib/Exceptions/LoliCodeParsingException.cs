using System;

namespace RuriLib.Exceptions
{
    public class LoliCodeParsingException : Exception
    {
        public int LineNumber { get; set; }

        public LoliCodeParsingException(int lineNumber)
        {
            LineNumber = lineNumber;
        }

        public LoliCodeParsingException(int lineNumber, string message)
            : base($"[Line {lineNumber}] {message}")
        {
            LineNumber = lineNumber;
        }

        public LoliCodeParsingException(int lineNumber, string message, Exception inner)
            : base($"[Line {lineNumber}] {message}", inner)
        {
            LineNumber = lineNumber;
        }
    }
}
