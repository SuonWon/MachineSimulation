using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Domain;
using Gateway.Messaging;
using Gateway.Processing;
using Microsoft.Extensions.Hosting;

namespace Gateway.Hosting
{
    public class GatewayWorker : BackgroundService
    {
        private readonly MqttPublisherService _publisher;
        private readonly GatewayProcessor _generator;

        public GatewayWorker(MqttPublisherService publisher, GatewayProcessor generator)
        {
            _publisher = publisher;
            _generator = generator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _publisher.ConnectAsync();

            // var trainId = "T100";
            // var operatingMode = "Running";
            // double speed = 80;

            var bogieIds = new [] { "B1", "B2", "B3", "B4" };

            while(!stoppingToken.IsCancellationRequested)
            {
                // foreach(var bogieId in bogieIds)
                // {
                //     var telemetry = _generator.Generate(trainId, bogieId, operatingMode, speed);
                //     await _publisher.PublishTelemetryAsync(telemetry);
                // }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}