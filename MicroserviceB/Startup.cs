using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventBus;
using EventBus.InterfacesAbstraction;
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

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var autofac = new AutofacServiceProvider(AutofacContainer);

            builder.Register(c => new InMemorySubscriptionsManager())
             .As<ISubscriptionsManager>()
             .InstancePerLifetimeScope();

            var rabbitMQConfigSection = Configuration.GetSection("RabbitMQ");

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQConfigSection["Host"],
                UserName = rabbitMQConfigSection["UserName"],
                Password = rabbitMQConfigSection["Password"],
                DispatchConsumersAsync = true
            };

            int retryCountOut = 5;
            Int32.TryParse(rabbitMQConfigSection["EventBusRetryCount"], out retryCountOut);


            builder.Register(c => new RabbitMQConnection(factory, c.ResolveOptional<ILogger<EventBusRabbitMQ>>(), retryCountOut))
              .As<IRabbitMQConnection>()
              .InstancePerLifetimeScope();

            builder.Register(c => new EventBusRabbitMQ(c.ResolveOptional<IRabbitMQConnection>(),
                c.ResolveOptional<ILifetimeScope>(), c.ResolveOptional<ISubscriptionsManager>(),
                c.ResolveOptional<ILogger<EventBusRabbitMQ>>(), retryCountOut, "eventBus"))
              .As<IEventBus>()
              .InstancePerLifetimeScope();

            builder.Register(c => new EventFromMicroserviceAHandler());

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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

            eventBus.Subscribe<EventFromMicroserviceA, EventFromMicroserviceAHandler>();
        }
    }
}
