using RuriLib.Models.Jobs;
using System.Collections.Generic;

namespace OpenBullet2.Services
{
    public class JobManagerService
    {
        public bool Initialized { get; set; } = false;
        public List<Job> Jobs { get; } = new List<Job>();
    }
}
