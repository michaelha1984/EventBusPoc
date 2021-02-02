using EventBus.Common.Events;
using System;

namespace JobScheduler.Events
{
    public class AmazonSesJobScheduledEvent : IntegrationEvent
    {
        public Guid JobId { get; set; }
    }
}
