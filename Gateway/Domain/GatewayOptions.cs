using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Domain
{
    public class GatewayOptions
    {
        public MqttOptions Mqtt { get; set; } = new();
    }

    public class MqttOptions
    {
        public string ClientId { get; set; } = "train-gateway-001";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1883;
    }
}