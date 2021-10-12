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
using MicroserviceB.Event;
using MicroserviceB.Handler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MicroserviceB
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

       
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var producerConfiguration = new ProducerConfig { BootstrapServers = "localhost:9092" };

            var consumerConfiguration = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "testa",
                AutoOffsetReset= AutoOffsetReset.Earliest
            };


            services.AddSingleton(new KafkaConnection(
         producerConfiguration
         , consumerConfiguration));

            services.AddSingleton<IEventBus, EventBusKafka>(sp =>
            {
                var kafkaConnection = sp.GetRequiredService<KafkaConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusKafka>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
                return new EventBusKafka(eventBusSubcriptionsManager, logger, kafkaConnection, sp);
            });



            services.AddTransient<EventFromMicroserviceAHandler>();
            services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();


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

            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<EventFromMicroserviceA, EventFromMicroserviceAHandler>();
        }
    }
}
