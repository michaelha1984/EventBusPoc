using MediatR;
using System.Collections.Generic;

namespace Application.Queries.GetScheduledJobs
{
    public class GetScheduledJobsQuery : IRequest<List<ScheduledJob>>
    {
    }
}
