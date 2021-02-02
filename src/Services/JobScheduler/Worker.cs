using System;
using System.Threading;
using System.Threading.Tasks;
using EventBus.Common.Abstractions;
using JobScheduler.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobScheduler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        //private readonly IMediator _mediator;
        private readonly IEventBus _eventBus;

        public Worker(
            ILogger<Worker> logger, 
            //IMediator mediator, 
            IEventBus eventBus)
        {
            _logger = logger;
            //_mediator = mediator;
            _eventBus = eventBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _eventBus.Subscribe<JobSentEvent>();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // TODO: Poll database for new jobs then distribute to 
                //var query = new GetScheduledJobsQuery();

                //var response = await _mediator.Send(query);

                //// TODO: Factory
                //foreach (var job in response)
                //{
                //    if (job.Type == "Amazon") // TODO: enum
                //    {
                        var startSes = new AmazonSesJobScheduledEvent
                        {
                            JobId = Guid.NewGuid()// job.JobId
                        };

                        _eventBus.Publish(startSes);
                //    }
                //}


                await Task.Delay(10000, stoppingToken);
                Console.WriteLine("Enter to publish");
                Console.ReadKey();
            }
        }
    }
}
