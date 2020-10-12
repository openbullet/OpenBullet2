using System;

namespace RuriLib.Models.Jobs.Threading
{
    public class RequiredStatusException : Exception
    {
        public RequiredStatusException(TaskManagerStatus requiredStatus, TaskManagerStatus actualStatus) 
            : base ($"The operation can only be performed when the Task Manager is in a {requiredStatus} status, but the status was {actualStatus}.")
        {

        }

        public RequiredStatusException(TaskManagerStatus[] requiredStatuses, TaskManagerStatus actualStatus)
            : base($"The operation can only be performed when the Task Manager is in one of these statuses: {string.Join(", ", requiredStatuses)}, but the status was {actualStatus}.")
        {

        }
    }
}
