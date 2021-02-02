using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.GetScheduledJobs
{
    public class GetScheduledJobsQueryHandler : IRequestHandler<GetScheduledJobsQuery, List<ScheduledJob>>
    {
        public Task<List<ScheduledJob>> Handle(GetScheduledJobsQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<ScheduledJob>
            {
                new ScheduledJob
                {
                    JobId = Guid.Parse("C385CC1F-835B-400F-8E1F-CF1176221BE3"),
                    Name = "Hello World",
                    Type = "Amazon" // TODO: Enum
                }
            });
        }
    }
}
