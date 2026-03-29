using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using BogieSimulator.Simulation;

var trainSimulator = new TrainSimulator("Train-01");
var host = "127.0.0.1";
var port = 6000;

var bogies = new List<BogieUnitSimulator>
{
    new("Bogie-01"),
    new("Bogie-02"),
    new("Bogie-03"),
    new("Bogie-04")
};

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = false
};

Console.WriteLine("Starting train-level shared mode bogie simulator...");
Console.WriteLine("Press Ctrl+C to stop.");

while (true)
{
    try
    {
        using var client = new TcpClient();
        Console.WriteLine("connecting to gateway...");
        await client.ConnectAsync(host, port);
        Console.WriteLine("Connected to Gateway.");

        using var stream = client.GetStream();

        while(true)
        {
            var context = trainSimulator.NextContext();

            Console.WriteLine(
                $"TRAIN | Mode={context.Mode} | BaseSpeed={context.BaseSpeedKph:F2} kph | BaseBrake={context.BaseBrakePressureBar:F2} bar");

            foreach (var bogie in bogies)
            {
                var telemetry = bogie.Next(context);
                var json = JsonSerializer.Serialize(telemetry, jsonOptions);
                var message = json + "\n";
                var bytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(bytes);
                Console.WriteLine($"Sent: {telemetry.BogieId} -> {json}");
            }
            await Task.Delay(1000);
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Connection lost: {ex.Message}");
        Console.WriteLine("Retrying in 3 seconds...");
        await Task.Delay(3000);
    }
}