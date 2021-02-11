using System;

namespace RuriLib.Parallelization.Exceptions
{
    public class RequiredStatusException : Exception
    {
        public RequiredStatusException(ParallelizerStatus requiredStatus, ParallelizerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in a {requiredStatus} status, but the status was {actualStatus}.")
        {

        }

        public RequiredStatusException(ParallelizerStatus[] requiredStatuses, ParallelizerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in one of these statuses: {string.Join(", ", requiredStatuses)}, but the status was {actualStatus}.")
        {

        }
    }
}
