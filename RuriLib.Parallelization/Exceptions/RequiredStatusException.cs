using System;

namespace RuriLib.Parallelization.Exceptions
{
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
        public RequiredStatusException(ParallelizerStatus requiredStatus, ParallelizerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in a {requiredStatus} status, but the status was {actualStatus}.")
        {

        }

        /// <summary>
        /// When the <see cref="Parallelizer{TInput, TOutput}"/> requires a status in the <paramref name="requiredStatuses"/> array
        /// but the status was <paramref name="actualStatus"/>.
        /// </summary>
        public RequiredStatusException(ParallelizerStatus[] requiredStatuses, ParallelizerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in one of these statuses: {string.Join(", ", requiredStatuses)}, but the status was {actualStatus}.")
        {

        }
    }
}
