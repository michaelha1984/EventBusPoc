using EventBus.Common.Events;
using System.Threading.Tasks;

namespace EventBus.Common.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }
    
    public interface IIntegrationEventHandler
    {

    }
}
