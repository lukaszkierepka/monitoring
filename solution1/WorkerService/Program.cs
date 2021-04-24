using System;
using Common;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using WorkerService.Order;

namespace WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddMassTransit(p =>
                    {
                        p.AddConsumer<OrderAggregate>();
                        p.UsingRabbitMq((context, configurator) =>
                        {
                            configurator.Host("rabbit", hostConfigurator =>
                            {
                                hostConfigurator.Username("guest");
                                hostConfigurator.Password("guest");
                            });
                            configurator.ConfigureEndpoints(context);
                        });
                    });
                    services.AddMassTransitHostedService();
                    services.AddOpenTelemetryTracing(builder =>
                        builder
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("workerservice"))
                            .AddOtlpExporter(options => options.Endpoint = new Uri("http://collector:4317"))
                            .AddMassTransitInstrumentation()
                            .AddConsoleExporter()
                    );
                    services.AddSingleton<IIdGenerator, SequentialIdGenerator>();
                })
                .UseSerilog((context, configuration) =>
                    configuration
                        .MinimumLevel.Information()
                        .WriteTo.Elasticsearch(
                            new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
                            {
                                AutoRegisterTemplate = true,
                                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                                FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                                EmitEventFailure = EmitEventFailureHandling.RaiseCallback,
                                
                            })
                        .WriteTo.Console());
    }
}