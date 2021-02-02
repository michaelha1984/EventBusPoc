using EventBus.Common.Events;
using System;

namespace AmazonSesWorker.Events
{
    public class JobSentEvent : IntegrationEvent
    {
        public Guid JobId { get; set; }
    }
}
