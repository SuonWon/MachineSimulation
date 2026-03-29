using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BogieSimulator.Simulation
{
    public sealed class TrainContext
    {
        public string TrainId {get; set;} = default!;
        public TrainOperatingMode Mode {get; set;}
        public double BaseSpeedKph {get; set;}
        public double BaseBrakePressureBar {get; set;}
        public DateTime TimestampUtc {get; set;}
    }
}