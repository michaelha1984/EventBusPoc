using AmazonSesWorker.Services;
using EventBus.RabbitMq.Extensions;
using IVE.Digital.Gateway.Application;
using IVE.Digital.Gateway.Application.Common.Interfaces;
using IVE.Digital.Gateway.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AmazonSesWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddRabbitMq();
                });
    }
}
