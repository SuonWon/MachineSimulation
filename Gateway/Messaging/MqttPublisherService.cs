using System.Text;
using System.Text.Json;
using Gateway.Domain;
using MQTTnet;
using MQTTnet.Protocol;

namespace Gateway.Messaging
{
    public class MqttPublisherService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttOptions _options;
        public MqttPublisherService(MqttOptions options)
        {
            _options = options;

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine("MQTT client disconnected. Attempting to reconnect...");
                await Task.Delay(TimeSpan.FromSeconds(3));
                await ConnectAsync();
            };
        }

        public async Task ConnectAsync(CancellationToken stoppingToken = default)
        {
            if(_mqttClient.IsConnected) return;

            var clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_options.ClientId)
                .WithTcpServer(_options.Host, _options.Port)
                .Build();

            await _mqttClient.ConnectAsync(clientOptions, stoppingToken);
            Console.WriteLine("MQTT client connected.");
        }

        public async Task PublishTelemetryAsync(BogieTelemetryMessage message, CancellationToken stoppingToken = default)
        {
            if (!_mqttClient.IsConnected) await ConnectAsync(stoppingToken);

            var topic = $"train/{message.TrainId}/bogie/{message.BogieId}/telemetry";
            var payload = JsonSerializer.Serialize(message);

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            
            await _mqttClient.PublishAsync(mqttMessage);
            Console.WriteLine($"Published: {topic} -> {payload}");
        }
    }
}