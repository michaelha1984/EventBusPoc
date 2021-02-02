using System;

namespace Application.Queries.GetScheduledJobs
{
    public class ScheduledJob
    {
        public Guid JobId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
