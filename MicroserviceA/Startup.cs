using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using EventBus;
using EventBus.InterfacesAbstraction;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MicroserviceA
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var autofac = new AutofacServiceProvider(AutofacContainer);

            builder.Register(c => new InMemorySubscriptionsManager())
             .As<ISubscriptionsManager>()
             .InstancePerLifetimeScope();

            //var rabbitMQConfigSection = Configuration.GetSection("RabbitMQ");

            //var factory = new ConnectionFactory()
            //{
            //    HostName = rabbitMQConfigSection["Host"],
            //    UserName = rabbitMQConfigSection["UserName"],
            //    Password = rabbitMQConfigSection["Password"],
            //    DispatchConsumersAsync = true
            //};

            //int retryCountOut = 5;
            //Int32.TryParse(rabbitMQConfigSection["EventBusRetryCount"], out retryCountOut);


            //builder.Register(c => new RabbitMQConnection(factory, c.ResolveOptional<ILogger<EventBusRabbitMQ>>(), retryCountOut))
            //  .As<IRabbitMQConnection>()
            //  .InstancePerLifetimeScope();

            //builder.Register(c => new EventBusRabbitMQ(c.ResolveOptional<IRabbitMQConnection>(),
            //    c.ResolveOptional<ILifetimeScope>(), c.ResolveOptional<ISubscriptionsManager>(),
            //    c.ResolveOptional<ILogger<EventBusRabbitMQ>>(), retryCountOut, "eventBus"))
            //  .As<IEventBus>()
            //  .InstancePerLifetimeScope();

           



            //builder.Register(c => new EventBusRabbitMQ(c.ResolveOptional<IRabbitMQConnection>(),
            //   c.ResolveOptional<ILifetimeScope>(), c.ResolveOptional<ISubscriptionsManager>(),
            //   c.ResolveOptional<ILogger<EventBusRabbitMQ>>(), retryCountOut, "eventBus"))
            // .As<IEventBus>()
            // .InstancePerLifetimeScope();


        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var producerConfiguration = new ProducerConfig { BootstrapServers = "localhost:9092" };
            var schemaRegistryConfiguration = new SchemaRegistryConfig
            {
                Url = "localhost:8081",
                RequestTimeoutMs = 5000,
                MaxCachedSchemas = 10
            };
            var avroSerializerConfiguration = new AvroSerializerConfig
            {
                // optional Avro serializer properties:
                // BufferBytes = 100,
                AutoRegisterSchemas = true,
            };

            var consumerConfiguration = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = Assembly.GetExecutingAssembly().GetName().Name
            };


            services.AddSingleton(new KafkaConnection(
         producerConfiguration
         , consumerConfiguration
         , schemaRegistryConfiguration
         , avroSerializerConfiguration));
            
            services.AddSingleton<IEventBus, EventBusKafka>(sp =>
            {
                var kafkaConnection = sp.GetRequiredService<KafkaConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusKafka>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                return new EventBusKafka(eventBusSubcriptionsManager, logger, kafkaConnection, sp);
            });



            //services.AddScoped<IEventBus, EventBusRabbitMQ>();
            services.AddScoped<IEventBus, EventBusKafka>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            var eventBus = AutofacContainer.Resolve<IEventBus>();
        }
    }
}
