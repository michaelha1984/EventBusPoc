using EventBus.Common.Abstractions;
using JobScheduler.Events;
using System;
using System.Threading.Tasks;

namespace JobScheduler.Handlers
{
    public class JobSentEventHandler : IIntegrationEventHandler<JobSentEvent>
    {
        public Task Handle(JobSentEvent @event)
        {
            // TODO : Set Job Status
            // TODO : Publish notification event

            Console.WriteLine("Good bye world");
            return Task.CompletedTask;
        }
    }
}
