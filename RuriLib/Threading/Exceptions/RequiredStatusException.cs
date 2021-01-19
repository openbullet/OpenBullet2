using System;

namespace RuriLib.Threading.Exceptions
{
    public class RequiredStatusException : Exception
    {
        public RequiredStatusException(ThreadManagerStatus requiredStatus, ThreadManagerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in a {requiredStatus} status, but the status was {actualStatus}.")
        {

        }

        public RequiredStatusException(ThreadManagerStatus[] requiredStatuses, ThreadManagerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in one of these statuses: {string.Join(", ", requiredStatuses)}, but the status was {actualStatus}.")
        {

        }
    }
}
