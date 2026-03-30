using System.Text;
using System.Text.Json;
using MQTTnet;
using TrainTelemetryConsumer;

var brokerHost = "localhost";
var brokerPort = 1883;
var clientId = $"TrainTelemetryConsumer-{Guid.NewGuid()}";

var factory = new MqttClientFactory();
var client = factory.CreateMqttClient();

client.ConnectedAsync += async e =>
{
    Console.WriteLine("Connected to MQTT broker.");
    var topicFilter = new MqttTopicFilterBuilder()
        .WithTopic("train/+/bogie/+/telemetry")
        .Build();

        await client.SubscribeAsync(topicFilter);

        Console.WriteLine("Subscribed to topic: train/+/bogie/+/telemetry");
};

client.DisconnectedAsync += async e =>
{
    Console.WriteLine("Disconnected from MQTT broker.");

    while(!client.IsConnected)
    {
        try
        {
            Console.WriteLine("Attempting to reconnect...");
            await Task.Delay(TimeSpan.FromSeconds(3));

            var reconnectOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerHost, brokerPort)
                .WithClientId(clientId)
                .Build();

            await client.ConnectAsync(reconnectOptions);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Reconnection failed: {ex.Message}");
        }
    }
};

client.ApplicationMessageReceivedAsync += e =>
{
    try
    {
        var topic = e.ApplicationMessage.Topic;
        var payloadByte = e.ApplicationMessage.Payload;
        var payload = Encoding.UTF8.GetString(payloadByte);

        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine($"Topic   : {topic}");
        Console.WriteLine($"Payload : {payload}");

        var message = JsonSerializer.Deserialize<BogieTelemetryMessage>(payload);
        if(message != null)
        {
            Console.WriteLine($"Train   : {message.TrainId}");
            Console.WriteLine($"Bogie   : {message.BogieId}");
            Console.WriteLine($"Mode    : {message.OperatingMode}");
            Console.WriteLine($"Speed   : {message.SpeedKph} kph");
            Console.WriteLine($"Temp    : {message.AxleTemperatureC} C");
            Console.WriteLine($"Vib     : {message.VibrationMmS} mm/s");
            Console.WriteLine($"Time    : {message.TimestampUtc:O}");
        }
        else
        {
            Console.WriteLine("Payload could not be deserialized.");
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Error processing message: {ex.Message}");
    }

    return Task.CompletedTask;
};

var options = new MqttClientOptionsBuilder()
    .WithTcpServer(brokerHost, brokerPort)
    .WithClientId(clientId)
    .Build();

await client.ConnectAsync(options);

Console.WriteLine("Press Ctrl+C to exit.");

var exitTcs = new TaskCompletionSource();
Console.CancelKeyPress += async(_, evengArgs) =>
{
    evengArgs.Cancel = true;

    try
    {
        if(client.IsConnected)
        {
            await client.DisconnectAsync();
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Error during disconnection: {ex.Message}");
    }
    finally
    {
        exitTcs.SetResult();
    }
};

await exitTcs.Task;