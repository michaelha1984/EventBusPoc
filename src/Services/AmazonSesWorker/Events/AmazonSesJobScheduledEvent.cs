using EventBus.Common.Events;
using System;

namespace AmazonSesWorker.Events
{
    public class AmazonSesJobScheduledEvent : IntegrationEvent
    {
        public Guid JobId { get; set; }
    }
}
