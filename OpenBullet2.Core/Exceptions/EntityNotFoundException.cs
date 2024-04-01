using System;

namespace OpenBullet2.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// Creates a new <see cref="EntityNotFoundException"/> with a message.
    /// </summary>
    public EntityNotFoundException(string message) : base(message) { }
}
