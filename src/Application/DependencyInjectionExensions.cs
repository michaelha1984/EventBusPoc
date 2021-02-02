// <copyright file="DependencyInjectionExensions.cs" company="IVE Group Limited">
// This source code is Copyright © IVE Group Limited and MAY NOT be copied, reproduced,
// published, distributed or transmitted to or stored in any manner without prior
// written consent from IVE Group Limited (www.ivegroup.com.au).
// </copyright>

namespace Application
{
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Dependency Injection for the application layer.
    /// </summary>
    public static class DependencyInjectionExensions
    {
        /// <summary>
        /// Adds the required services for the application layer.
        /// </summary>
        /// <param name="services">Service Collection to add to.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
