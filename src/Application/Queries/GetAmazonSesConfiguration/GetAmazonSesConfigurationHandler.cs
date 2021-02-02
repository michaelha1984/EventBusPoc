using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Queries.GetAmazonSesConfiguration
{

    public class GetAmazonSesConfigurationHandler :
        IRequestHandler<GetAmazonSesConfigurationQuery, AmazonSesConfiguration>
    {
        public Task<AmazonSesConfiguration> Handle(GetAmazonSesConfigurationQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new AmazonSesConfiguration
                {
                    AccessKey = "ABC123",
                    SecretKey = "ABC123"
                });
        }
    }
}
