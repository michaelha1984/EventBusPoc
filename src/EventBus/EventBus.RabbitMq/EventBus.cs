using EventBus.Common.Abstractions;
using EventBus.Common.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMq
{
    public class EventBus : IEventBus
    {
        private readonly ILogger<EventBus> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPersistConnection _connection;
        private readonly string _queueName;
        private IModel _consumerChannel;

        private Dictionary<string, Type> _subscribers = new Dictionary<string, Type>();

        public EventBus(
            ILogger<EventBus> logger,
            IServiceScopeFactory scopeFactory,
            IPersistConnection connection,
            string queueName = "test")
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _connection = connection;
            _queueName = queueName;

            _consumerChannel = CreateConsumerChannel();
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
                });

            var eventName = @event.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, eventName);

            using var channel = _connection.CreateModel();

            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

            channel.ExchangeDeclare(exchange: "EXCHANGE_NAME", type: "direct");

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                channel.BasicPublish(
                    exchange: "EXCHANGE_NAME",
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }

        public void Subscribe<T>()
            where T : IntegrationEvent
        {
            var eventName = typeof(T).Name;

            _subscribers.Add(eventName, typeof(IIntegrationEventHandler<T>)); // TODO: 
            //var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);

            //if (!containsKey)
            //{
                if (!_connection.IsConnected)
                {
                    _connection.TryConnect();
                }

                using (var channel = _connection.CreateModel())
                {
                    channel.QueueBind(queue: _queueName,
                                      exchange: "EXCHANGE_NAME",
                                      routingKey: eventName);
                }
            //}

            _logger.LogInformation("Subscribing to event {EventName}", 
                eventName);

            StartBasicConsume();
        }

        private IModel CreateConsumerChannel()
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: "EXCHANGE_NAME",
                                    type: "direct");

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            channel.CallbackException += (sender, eventArgs) =>
            {
                _logger.LogWarning(eventArgs.Exception, "Recreating RabbitMQ consumer channel");

                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
            }

            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subscribers.ContainsKey(eventName))
            {
                using var scope = _scopeFactory.CreateScope();
                //var handlerType = typeof(IIntegrationEventHandler<>)
                //    .MakeGenericType(eventName.GetType());

                var handlerType = _subscribers[eventName];
                var handler = scope.ServiceProvider.GetRequiredService(handlerType);
                var eventType = handlerType.GetGenericArguments().Single();
                var method = handler.GetType().GetMethod("Handle");

                //var callingAssembly = Assembly.GetCallingAssembly();
                //var types = callingAssembly.DefinedTypes.ToList();
                //var type = callingAssembly.DefinedTypes.Single(t => t.AsType() == Type.GetType(eventName));

                var obj = JsonConvert.DeserializeObject(message, eventType);

                //var typedObj = Convert.ChangeType(obj, Type.GetType(eventName));

                await (Task)method.Invoke(handler, new object[] { obj });

                //var handler = scope.ResolveOptional(subscription.HandlerType);
                //if (handler == null) continue;
                //var eventType = _subsManager.GetEventTypeByName(eventName);
                //var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                //var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                //await Task.Yield();
                //await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });

                //if (_subsManager.HasSubscriptionsForEvent(eventName))
                //{
                //    using (var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME))
                //    {
                //        var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                //        foreach (var subscription in subscriptions)
                //        {
                //            var handler = scope.ResolveOptional(subscription.HandlerType);
                //            if (handler == null) continue;
                //            var eventType = _subsManager.GetEventTypeByName(eventName);
                //            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                //            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                //            await Task.Yield();

                //            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                //        }
                //    }
                //}
                //else
                //{
                //    _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
                //}
            }
            else 
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }
        }
    }
}
