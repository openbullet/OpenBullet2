using System;

namespace RuriLib.Exceptions
{
    /// <summary>
    /// An exception that is thrown when no Wordlist Types were specified
    /// in the Environment settings.
    /// </summary>
    public class NoWordlistTypesExceptions : Exception
    {
        public NoWordlistTypesExceptions() : base("No Wordlist Types specified in the Environment settings")
        {

        }
    }
}
