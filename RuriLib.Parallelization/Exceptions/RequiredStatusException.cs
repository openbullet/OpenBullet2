using System;
using System.Linq;

namespace RuriLib.Parallelization.Exceptions;

/// <summary>
/// Exception that is thrown when a method of a <see cref="Parallelizer{TInput, TOutput}"/> can only
/// be executed when its status has one of the specified <see cref="ParallelizerStatus"/> values.
/// </summary>
public class RequiredStatusException : Exception
{
    /// <summary>
    /// When the <see cref="Parallelizer{TInput, TOutput}"/> requires a status of <paramref name="requiredStatus"/>
    /// but the status was <paramref name="actualStatus"/>.
    /// </summary>
    private RequiredStatusException(ParallelizerStatus actualStatus, ParallelizerStatus requiredStatus)
        : base($"The operation can only be performed when the Task Manager is in a {requiredStatus} status, but the status was {actualStatus}.")
    {

    }

    /// <summary>
    /// When the <see cref="Parallelizer{TInput, TOutput}"/> requires a status in the <paramref name="requiredStatuses"/> array
    /// but the status was <paramref name="actualStatus"/>.
    /// </summary>
    private RequiredStatusException(ParallelizerStatus actualStatus, ParallelizerStatus[] requiredStatuses)
        : base($"The operation can only be performed when the Task Manager is in one of these statuses: {string.Join(", ", requiredStatuses)}, but the status was {actualStatus}.")
    {

    }
        
    /// <summary>
    /// Throws a <see cref="RequiredStatusException"/> if the <paramref name="actualStatus"/> is not equal to the <paramref name="requiredStatus"/>.
    /// </summary>
    public static void ThrowIfNot(ParallelizerStatus actualStatus, ParallelizerStatus requiredStatus)
    {
        if (actualStatus != requiredStatus)
        {
            throw new RequiredStatusException(actualStatus, requiredStatus);
        }
    }
        
    /// <summary>
    /// Throws a <see cref="RequiredStatusException"/> if the <paramref name="actualStatus"/> is not one of the <paramref name="requiredStatuses"/>.
    /// </summary>
    public static void ThrowIfNot(ParallelizerStatus actualStatus, ParallelizerStatus[] requiredStatuses)
    {
        if (!requiredStatuses.Contains(actualStatus))
        {
            throw new RequiredStatusException(actualStatus, requiredStatuses);
        }
    }
}
