namespace BogieSimulator.Simulation;

public sealed class TrainSimulator
{
    private readonly Random _random = new();
    private readonly string _trainId;

    private TrainOperatingMode _mode = TrainOperatingMode.Idle;
    private double _baseSpeedKph = 0;
    private double _baseBrakePressureBar = 0;
    private int _tickCount = 0;

    public TrainSimulator(string trainId)
    {
        _trainId = trainId;
    }

    public TrainContext NextContext()
    {
        _tickCount++;

        MaybeChangeMode();
        UpdateTrainState();

        return new TrainContext
        {
            TrainId = _trainId,
            Mode = _mode,
            BaseSpeedKph = Math.Round(_baseSpeedKph, 2),
            BaseBrakePressureBar = Math.Round(_baseBrakePressureBar, 2),
            TimestampUtc = DateTime.UtcNow
        };
    }

    private void MaybeChangeMode()
    {
        if (_tickCount % 20 != 0)
            return;

        var roll = _random.Next(1, 101);

        _mode = roll switch
        {
            <= 10 => TrainOperatingMode.Idle,
            <= 70 => TrainOperatingMode.Running,
            <= 92 => TrainOperatingMode.Braking,
            _ => TrainOperatingMode.Fault
        };
    }

    private void UpdateTrainState()
    {
        switch (_mode)
        {
            case TrainOperatingMode.Idle:
                _baseSpeedKph = MoveTowards(_baseSpeedKph, 0, 10);
                _baseBrakePressureBar = RandomRange(0.0, 0.3);
                break;

            case TrainOperatingMode.Running:
                _baseSpeedKph = MoveTowards(_baseSpeedKph, RandomRange(60, 110), 5);
                _baseBrakePressureBar = RandomRange(0.0, 1.0);
                break;

            case TrainOperatingMode.Braking:
                _baseSpeedKph = MoveTowards(_baseSpeedKph, RandomRange(0, 30), 12);
                _baseBrakePressureBar = RandomRange(3.5, 6.5);
                break;

            case TrainOperatingMode.Fault:
                _baseSpeedKph = MoveTowards(_baseSpeedKph, RandomRange(0, 40), 15);
                _baseBrakePressureBar = RandomRange(4.0, 7.0);
                break;
        }
    }

    private double RandomRange(double min, double max)
        => min + (_random.NextDouble() * (max - min));

    private static double MoveTowards(double current, double target, double maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;

        return current + Math.Sign(target - current) * maxDelta;
    }
}