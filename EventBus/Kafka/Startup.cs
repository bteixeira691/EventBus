using Confluent.Kafka;
using EventBus.InterfacesAbstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Kafka
{
    public static class Startup
    {
        public static void AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            var b = configuration.GetSection("Kafka:ProducerConfig");

            ProducerConfig producerConfiguration = (ProducerConfig)configuration.GetSection("Kafka:ProducerConfig");

            ConsumerConfig consumerConfiguration = (ConsumerConfig)configuration.GetSection("Kafka:ConsumerConfig");

            services.AddSingleton(new KafkaConnection(producerConfiguration, consumerConfiguration));

            services.AddSingleton<IEventBus, EventBusKafka>(sp =>
            {
                var kafkaConnection = sp.GetRequiredService<KafkaConnection>();
                var logger = sp.GetRequiredService<ILogger>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                return new EventBusKafka(eventBusSubcriptionsManager, logger, kafkaConnection, sp);
            });




            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();

        }
    }
}
