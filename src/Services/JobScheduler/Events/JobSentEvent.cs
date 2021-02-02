using EventBus.Common.Events;
using System;

namespace JobScheduler.Events
{
    public class JobSentEvent : IntegrationEvent
    {
        public Guid JobId { get; set; }
    }
}
