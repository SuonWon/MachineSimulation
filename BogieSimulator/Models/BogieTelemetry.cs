using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BogieSimulator.Models
{
    public sealed class BogieTelemetry
    {
        public long Sequence { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string TrainId { get; set; } = default!;
        public string BogieId { get; set; } = default!;
        public string TrainMode { get; set; } = default!;
        public string BogieStatus { get; set; } = default!;
        public double SpeedKph { get; set; }
        public double WheelRpm { get; set; }
        public double AxleTemperatureC { get; set; }
        public double VibrationMmS { get; set; }
        public double BrakePressureBar { get; set; }
        public bool SlipDetected { get; set; }
        public string? AlarmCode { get; set; }
    }
}