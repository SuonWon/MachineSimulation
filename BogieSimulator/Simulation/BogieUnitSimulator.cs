using BogieSimulator.Models;

namespace BogieSimulator.Simulation;

public sealed class BogieUnitSimulator
{
    private readonly Random _random;
    private readonly string _bogieId;

    private double _axleTempC;
    private double _vibrationMmS;
    private bool _hasPersistentFault;
    private string? _persistentAlarmCode;

    private long _sequence = 0;

    public BogieUnitSimulator(string bogieId)
    {
        _bogieId = bogieId;
        _random = new Random(Guid.NewGuid().GetHashCode());

        _axleTempC = RandomRange(30, 34);
        _vibrationMmS = RandomRange(0.2, 0.6);
    }

    public BogieTelemetry Next(TrainContext context)
    {
        _sequence++;

        MaybeInjectLocalFault();

        var speedKph = CalculateLocalSpeed(context);
        var wheelRpm = Math.Max(0, SpeedToRpm(speedKph) + Noise(5));
        var brakePressureBar = CalculateLocalBrakePressure(context);
        var slipDetected = CalculateSlip(context, speedKph);

        UpdateTemperature(context, speedKph);
        UpdateVibration(context);

        var bogieStatus = GetBogieStatus(context, slipDetected);
        var alarmCode = GetAlarmCode(context, slipDetected);

        return new BogieTelemetry
        {
            Sequence = _sequence,
            TimestampUtc = context.TimestampUtc,
            TrainId = context.TrainId,
            BogieId = _bogieId,
            TrainMode = context.Mode.ToString(),
            BogieStatus = bogieStatus,
            SpeedKph = Math.Round(speedKph, 2),
            WheelRpm = Math.Round(wheelRpm, 2),
            AxleTemperatureC = Math.Round(_axleTempC, 2),
            VibrationMmS = Math.Round(_vibrationMmS, 2),
            BrakePressureBar = Math.Round(brakePressureBar, 2),
            SlipDetected = slipDetected,
            AlarmCode = alarmCode
        };
    }

    private double CalculateLocalSpeed(TrainContext context)
    {
        var variation = context.Mode switch
        {
            TrainOperatingMode.Idle => Noise(0.5),
            TrainOperatingMode.Running => Noise(2.0),
            TrainOperatingMode.Braking => Noise(2.5),
            TrainOperatingMode.Fault => Noise(4.0),
            _ => 0
        };

        var speed = context.BaseSpeedKph + variation;

        if (_persistentAlarmCode == "CRITICAL_WHEEL_SLIP")
        {
            speed -= RandomRange(3, 8);
        }

        return Math.Max(0, speed);
    }

    private double CalculateLocalBrakePressure(TrainContext context)
    {
        var pressure = context.BaseBrakePressureBar + Noise(0.4);

        if (_persistentAlarmCode == "BRAKE_PRESSURE_ANOMALY")
        {
            pressure += RandomRange(1.5, 2.5);
        }

        return Math.Max(0, pressure);
    }

    private bool CalculateSlip(TrainContext context, double speedKph)
    {
        if (_persistentAlarmCode == "CRITICAL_WHEEL_SLIP")
            return true;

        return context.Mode switch
        {
            TrainOperatingMode.Braking => _random.NextDouble() < 0.10,
            TrainOperatingMode.Fault => _random.NextDouble() < 0.18,
            TrainOperatingMode.Running => _random.NextDouble() < 0.02,
            _ => false
        };
    }

    private void UpdateTemperature(TrainContext context, double speedKph)
    {
        var target = context.Mode switch
        {
            TrainOperatingMode.Idle => RandomRange(30, 36),
            TrainOperatingMode.Running => 42 + (speedKph / 10) + Noise(1.5),
            TrainOperatingMode.Braking => 48 + Noise(2.0),
            TrainOperatingMode.Fault => 55 + Noise(4.0),
            _ => 35
        };

        if (_persistentAlarmCode == "AXLE_OVERHEAT")
        {
            target += RandomRange(20, 35);
        }

        _axleTempC = MoveTowards(_axleTempC, target, 1.5);
    }

    private void UpdateVibration(TrainContext context)
    {
        var target = context.Mode switch
        {
            TrainOperatingMode.Idle => RandomRange(0.2, 0.8),
            TrainOperatingMode.Running => RandomRange(0.8, 2.0),
            TrainOperatingMode.Braking => RandomRange(1.2, 3.0),
            TrainOperatingMode.Fault => RandomRange(2.5, 4.5),
            _ => 0.5
        };

        if (_persistentAlarmCode == "BEARING_VIBRATION_HIGH")
        {
            target += RandomRange(3.0, 5.0);
        }

        _vibrationMmS = MoveTowards(_vibrationMmS, target, 0.8);
    }

    private string GetBogieStatus(TrainContext context, bool slipDetected)
    {
        if (_persistentAlarmCode is not null || slipDetected)
            return "Fault";

        return context.Mode switch
        {
            TrainOperatingMode.Idle => "Idle",
            TrainOperatingMode.Running => "Running",
            TrainOperatingMode.Braking => "Braking",
            TrainOperatingMode.Fault => "Warning",
            _ => "Unknown"
        };
    }

    private string? GetAlarmCode(TrainContext context, bool slipDetected)
    {
        if (_persistentAlarmCode is not null)
            return _persistentAlarmCode;

        if (slipDetected)
        {
            return context.Mode switch
            {
                TrainOperatingMode.Braking => "WHEEL_SLIDE_DETECTED",
                TrainOperatingMode.Fault => "CRITICAL_WHEEL_SLIP",
                TrainOperatingMode.Running => "WHEEL_SLIP_WARN",
                _ => null
            };
        }

        return null;
    }

    private void MaybeInjectLocalFault()
    {
        if (_hasPersistentFault)
        {
            if (_random.NextDouble() < 0.05)
            {
                _hasPersistentFault = false;
                _persistentAlarmCode = null;
            }

            return;
        }

        if (_random.NextDouble() < 0.02)
        {
            _hasPersistentFault = true;

            _persistentAlarmCode = _random.Next(1, 5) switch
            {
                1 => "AXLE_OVERHEAT",
                2 => "BEARING_VIBRATION_HIGH",
                3 => "CRITICAL_WHEEL_SLIP",
                _ => "BRAKE_PRESSURE_ANOMALY"
            };
        }
    }

    private static double SpeedToRpm(double speedKph) => speedKph * 13.5;

    private double Noise(double amplitude)
        => (_random.NextDouble() * 2 - 1) * amplitude;

    private double RandomRange(double min, double max)
        => min + (_random.NextDouble() * (max - min));

    private static double MoveTowards(double current, double target, double maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;

        return current + Math.Sign(target - current) * maxDelta;
    }
}