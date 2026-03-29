
using Gateway.Domain;
using Gateway.Hosting;
using Gateway.Messaging;
using Gateway.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    var options = context.Configuration.Get<GatewayOptions>() ?? new GatewayOptions();

    services.AddSingleton(options);
    services.AddSingleton(options.Mqtt);

    services.AddSingleton<MqttPublisherService>();
    services.AddSingleton<GatewayProcessor>();

    services.AddHostedService<TcpTelemetryServer>();
}).Build();

await host.RunAsync();