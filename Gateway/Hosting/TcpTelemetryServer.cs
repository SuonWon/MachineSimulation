using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Gateway.Processing;
using Gateway.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Gateway.Domain;

namespace Gateway.Hosting
{
    public class TcpTelemetryServer: BackgroundService
    {
        private readonly GatewayProcessor _processor;
        private readonly MqttPublisherService _publisher;

        private readonly string _listenIp = "0.0.0.0";
        private readonly int _listenPort = 6000;
        private readonly string _operatingMode = "Running";

        public TcpTelemetryServer(GatewayProcessor processor, MqttPublisherService publisher)
        {
            _processor = processor;
            _publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _publisher.ConnectAsync(stoppingToken);

            var ipAddress = IPAddress.Parse(_listenIp);
            var listener = new TcpListener(ipAddress, _listenPort);
            listener.Start();
            Console.WriteLine($"Gateway TCP server is listening on {_listenIp}:{_listenPort}");

            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    var tcpClient = await listener.AcceptTcpClientAsync(stoppingToken);
                    Console.WriteLine("Simulator connected.");

                    _ = Task.Run(() => ClientHandleAsync(tcpClient, stoppingToken), stoppingToken);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task ClientHandleAsync(TcpClient client, CancellationToken stoppingToken)
        {
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    var reader = new StreamReader(stream, Encoding.UTF8);

                    while(!stoppingToken.IsCancellationRequested)
                    {
                        var line = await reader.ReadLineAsync(stoppingToken);
                        if(line is null)
                        {
                            Console.WriteLine("Simulator disconnected.");
                            break;
                        }
                        if(string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var raw = JsonSerializer.Deserialize<RawBogieData>(line);
                            if(raw is null)
                            {
                                Console.WriteLine("Received invalid payload: deserialized to null.");
                                continue;
                            }
                            Console.WriteLine($"Received: {line}");

                            var message = _processor.RawDataProcess(raw, _operatingMode);
                            await _publisher.PublishTelemetryAsync(message, stoppingToken);
                        }
                        catch(JsonException ex)
                        {
                            Console.WriteLine($"Received invalid JSON payload: {ex.Message}");
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Error processing payload: {ex.Message}");
                        }
                    }
                }
            }
            catch(OperationCanceledException) { }
            catch(Exception ex)
            {
                Console.WriteLine($"Client handler error: {ex.Message}");
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}