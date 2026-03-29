using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Domain
{
    public class BogieTelemetryMessage
    {
        public string TrainId { get; set; } = default!;
        public string BogieId { get; set; } = default!;
        public string OperatingMode { get; set; } = default!;
        public double SpeedKph { get; set; }
        public double AxleTemperatureC { get; set; }
        public double VibrationMmS { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}