using OpenBullet2.Models.Jobs;
using System;

namespace OpenBullet2.Entities
{
    public class JobEntity : Entity
    {
        public int OwnerId { get; set; }
        public DateTime CreationDate { get; set; }
        public JobType JobType { get; set; }
        public string JobOptions { get; set; } // JSON
    }
}
