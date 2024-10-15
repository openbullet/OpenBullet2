using System;

namespace RuriLib.Exceptions;

/// <summary>
/// An exception that is thrown when a Wordlist Type with the given name was not present
/// in the Environment settings.
/// </summary>
public class InvalidWordlistTypeException : Exception
{
    /// <summary>
    /// Creates a <see cref="InvalidWordlistTypeException"/> with a message that contains the invalid type.
    /// </summary>
    /// <param name="type">
    /// The invalid Wordlist Type that was not found in the Environment settings.
    /// </param>
    public InvalidWordlistTypeException(string type) 
        : base($"The Wordlist Type {type} was not found in the Environment settings")
    {

    }
}
