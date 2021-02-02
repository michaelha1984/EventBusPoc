using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using EventBus.Common.Extensions;
using System.Reflection;
using EventBus.Common.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IPersistConnection>(sp => 
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    DispatchConsumersAsync = true
                };

                var logger = sp.GetRequiredService<ILogger<PersistConnection>>();

                return new PersistConnection(factory, logger, 5);
            });

            services.AddSingleton<IEventBus, EventBus>();

            var assembly = Assembly.GetCallingAssembly();
            services.AddIntegrationEventHandlers(new List<Assembly> { assembly });

            return services;
        }
    }
}
