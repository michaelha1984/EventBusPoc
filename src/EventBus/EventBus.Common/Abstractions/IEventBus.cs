using EventBus.Common.Events;

namespace EventBus.Common.Abstractions
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event);

        void Subscribe<T>()
            where T : IntegrationEvent;
    }
}
