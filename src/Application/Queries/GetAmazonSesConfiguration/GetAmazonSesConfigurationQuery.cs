using MediatR;
using System;

namespace Application.Queries.GetAmazonSesConfiguration
{
    public class GetAmazonSesConfigurationQuery : IRequest<AmazonSesConfiguration>
    {
        public Guid JobId { get; set; }
    }
}
