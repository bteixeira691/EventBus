using EventBus.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MicroserviceA
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //var producerConfiguration = new ProducerConfig 
            //{ 
            //    BootstrapServers = "localhost:9092" 
            //};

            //var consumerConfiguration = new ConsumerConfig
            //{
            //    BootstrapServers = "localhost:9092",
            //    GroupId = Assembly.GetExecutingAssembly().GetName().Name
            //};


            //services.AddSingleton(new KafkaConnection(
            //producerConfiguration
            //,consumerConfiguration));

            //services.AddSingleton<IEventBus, EventBusKafka>(sp =>
            //{
            //    var kafkaConnection = sp.GetRequiredService<KafkaConnection>();
            //    var logger = sp.GetRequiredService<ILogger>();
            //    var eventBusSubcriptionsManager = sp.GetRequiredService<ISubscriptionsManager>();
            //    return new EventBusKafka(eventBusSubcriptionsManager, logger, kafkaConnection, sp);
            //});

            ////services.AddTransient<OrderStatusChangedToStockConfirmedIntegrationEventHandler>();
            //services.AddSingleton<ISubscriptionsManager, InMemorySubscriptionsManager>();

            //services.AddScoped<IEventBus, EventBusRabbitMQ>();

            services.AddKafka(Configuration);
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

           
        }
    }
}
