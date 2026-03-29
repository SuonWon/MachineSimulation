using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Domain;

namespace Gateway.Processing
{
    public class GatewayProcessor
    {
        public BogieTelemetryMessage RawDataProcess(RawBogieData rawData, string operatingMode)
        {
            if(string.IsNullOrWhiteSpace(rawData.TrainId))
                throw new ArgumentException("TrainId cannot be null or empty.", nameof(rawData.TrainId));
            if(string.IsNullOrWhiteSpace(rawData.BogieId))
                throw new ArgumentException("BogieId cannot be null or empty.", nameof(rawData.BogieId));
            
            if(rawData.SpeedKph <0 ) rawData.SpeedKph = 0;
            if(rawData.AxleTemperatureC < -50) rawData.AxleTemperatureC = -50;
            if(rawData.VibrationMms < 0) rawData.VibrationMms = 0;

            return new BogieTelemetryMessage
            {
                TrainId = rawData.TrainId,
                BogieId = rawData.BogieId,
                OperatingMode = operatingMode,
                SpeedKph = rawData.SpeedKph,
                AxleTemperatureC = rawData.AxleTemperatureC,
                VibrationMmS = rawData.VibrationMms,
                TimestampUtc = rawData.TimestampUtc
            };
        }
    }
}