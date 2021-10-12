using Autofac;
using Confluent.Kafka;
using EventBus.Events;
using EventBus.InterfacesAbstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace EventBus.Kafka
{
    public class EventBusKafka : IEventBus
    {

        private readonly ISubscriptionsManager _subscriptionManager;
        private readonly ILogger<EventBusKafka> _logger;
        private readonly KafkaConnection _kafkaConnection;
        private readonly IServiceScopeFactory _serviceScopeFactory;



        public EventBusKafka(ISubscriptionsManager subscriptionManager, ILogger<EventBusKafka> logger,
            KafkaConnection kafkaConnection, IServiceProvider serviceProvider)
        {

            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _kafkaConnection = kafkaConnection ?? throw new ArgumentNullException(nameof(kafkaConnection));
            _serviceScopeFactory = serviceProvider?.GetRequiredService<IServiceScopeFactory>() 
                                ?? throw new ArgumentException($"Cannot resolve IServiceScopeFactory from {nameof(serviceProvider)}");
        }



        public async Task Publish(Event _event)
        {
            if (_event == null)
            {
                _logger.LogWarning("Event is null");
                return;
            }

            var eventName = _event.GetType().Name;

            try
            {
                var producer = _kafkaConnection.ProducerBuilder<Event>();

                _logger.LogInformation($"Publishing the event {_event} to Kafka topic {eventName}");
                var producerResult = await producer.ProduceAsync(eventName, new Message<Null, Event>() { Value = _event });

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occured during publishing the event to topic {_event}");
                _logger.LogError($"Consume exception {e.Message}, StackTrace {e.StackTrace}");
            }
        }


        public async Task Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;

            using (var consumer = _kafkaConnection.ConsumerBuilder<T>())
            {
                //subscribe the handler to the event
                _subscriptionManager.AddSubscription<T, TH>();

                consumer.Subscribe(eventName);

                //create a task to listen to the topic
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            _logger.LogInformation($"Consuming from topic {eventName}");

                            var consumerResult = consumer.Consume();
                            await ProcessEvent(consumerResult.Message.Value);
                        }
                        catch (ConsumeException e)
                        {
                            _logger.LogError($"Error `{e.Error.Reason}` occured during consuming the event from topic {eventName}");
                            _logger.LogError($"Consume exception {e.Message}, StackTrace {e.StackTrace}");

                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"Error `{e.Message}` occured during consuming the event from topic {eventName}");
                        }
                    }
                });
            }
        }

        private async Task ProcessEvent<T>(T message) where T : Event
        {


            var eventName = message.GetType().Name;

            _logger.LogInformation($"Process Event {eventName}");
            try
            {
                if (_subscriptionManager.HasSubscriptionsForEvent(eventName))
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var subscriptions = _subscriptionManager.GetHandlersForEvent(eventName);

                        _logger.LogInformation($"Subscriptions number {subscriptions.Count()}");

                        foreach (var subscription in subscriptions)
                        {
                            var handler = scope.ServiceProvider.GetRequiredService(subscription.HandlerType);

                            if (handler == null)
                                continue;

                            var eventType = _subscriptionManager.GetEventTypeByName(eventName);
                            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);


                            await Task.Yield();
                            await (Task)concreteType.GetMethod("Handler").Invoke(handler, new object[] { message });
                        }
                    }
                }
            }catch(Exception e)
            {

                _logger.LogError($"Process Event has an error {e.Message}, StackTrace {e.StackTrace}");
            }
        }


    }
}

