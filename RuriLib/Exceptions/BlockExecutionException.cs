using System;

namespace RuriLib.Exceptions;

public class BlockExecutionException : Exception
{
    public BlockExecutionException(string message)
        : base(message)
    {

    }
    
    public BlockExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
