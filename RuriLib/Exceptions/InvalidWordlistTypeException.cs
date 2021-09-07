using System;

namespace RuriLib.Exceptions
{
    /// <summary>
    /// An exception that is thrown when a Wordlist Type with the given name was not present
    /// in the Environment settings.
    /// </summary>
    public class InvalidWordlistTypeException : Exception
    {
        public InvalidWordlistTypeException(string type) 
            : base($"The Wordlist Type {type} was not found in the Environment settings")
        {

        }
    }
}
