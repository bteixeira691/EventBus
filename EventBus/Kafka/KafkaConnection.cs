using Confluent.Kafka;
using System;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Confluent.Kafka.SyncOverAsync;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Kafka
{
    public class KafkaConnection 
    {
        private readonly ProducerConfig _producerConfiguration;
        private readonly ConsumerConfig _consumerConfiguration;
        private readonly AvroSerializerConfig _avroSerializerConfiguration;
        private object _producerBuilder;

        public KafkaConnection(ProducerConfig producerConfig, ConsumerConfig consumerConfig,
              AvroSerializerConfig avroSerializerConfig)
        {


            this._producerConfiguration = producerConfig ?? throw new ArgumentNullException(nameof(producerConfig));
            this._consumerConfiguration = consumerConfig ?? throw new ArgumentNullException(nameof(consumerConfig));
                       this._avroSerializerConfiguration = avroSerializerConfig ?? throw new ArgumentNullException(nameof(avroSerializerConfig));

        }

        public IProducer<Null, T> ProducerBuilder<T>()
        {
            if (_producerBuilder == null)
            {

                _producerBuilder = new ProducerBuilder<Null, T>(_producerConfiguration)
                             //.SetKeySerializer(new AvroSerializer<string>(schemaRegistry))

                            .Build();
            }
            return (IProducer<Null, T>)_producerBuilder;
        }

        public IConsumer<Null, T> ConsumerBuilder<T>()
        {

            var consumer = new ConsumerBuilder<Null, T>(_consumerConfiguration)
                //.SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistry).AsSyncOverAsync())

                .Build();
            return consumer;
        }


    }
}
