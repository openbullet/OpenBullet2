using OpenBullet2.Models.Jobs;
using System;

namespace OpenBullet2.Entities
{
    public class JobEntity : Entity
    {
        public DateTime CreationDate { get; set; }
        public JobType JobType { get; set; }
        public string JobOptions { get; set; } // JSON

        public GuestEntity Owner { get; set; } // The owner of the job (null if admin)
    }
}
