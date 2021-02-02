using AmazonSesWorker.Events;
using Application.Queries.GetAmazonSesConfiguration;
using EventBus.Common.Abstractions;
using MediatR;
using System;
using System.Threading.Tasks;

namespace AmazonSesWorker.Handlers
{
    public class AmazonSesJobScheduledEventHandler : IIntegrationEventHandler<AmazonSesJobScheduledEvent>
    {
        private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;

        public AmazonSesJobScheduledEventHandler(
            IMediator mediator,
            IEventBus eventBus)
        {
            _mediator = mediator;
            _eventBus = eventBus;
        }

        public async Task Handle(AmazonSesJobScheduledEvent @event)
        {
            var query = new GetAmazonSesConfigurationQuery
            {
                JobId = @event.JobId
            };

            var response = await _mediator.Send(query);

            // Call AWS
            Console.WriteLine(response.AccessKey);
            Console.WriteLine(response.SecretKey);

            var jobSent = new JobSentEvent
            {
                JobId = @event.JobId
            };

            _eventBus.Publish(jobSent);
        }
    }
}
