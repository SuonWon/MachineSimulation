using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Domain
{
    public class RawBogieData
    {
        public string TrainId { get; set; } = default!;
        public string BogieId { get; set; } = default!;
        public double SpeedKph { get; set; }
        public double AxleTemperatureC { get; set; }
        public double VibrationMms { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}