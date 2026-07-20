using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Models;
using AmpClean.Domain.ValueObjects;

namespace AmpClean.Infrastructure.Simulation;

/// <summary>
/// 假仪器：根据三轴坐标生成 8 个带微小噪声的数据。
/// 输出契约与真实仪器一致，因此切换硬件不会影响状态机和 ViewModel。
/// </summary>
public sealed class FakeMeasurementInstrument(SimulationOptions options) : IMeasurementInstrument
{
    private readonly Random _random = new();
    private readonly TimeSpan _measureDuration =
        TimeSpan.FromMilliseconds(Math.Max(0, options.MeasureDurationMilliseconds));

    public async Task<MeasurementReading> MeasureAsync(
        AxisPosition position,
        CancellationToken cancellationToken = default)
    {
        if (_measureDuration > TimeSpan.Zero)
            await Task.Delay(_measureDuration, cancellationToken);
        var basis = 50D + position.X * 0.12D + position.Y * 0.08D + position.Z * 0.03D;
        var values = Enumerable.Range(0, 8)
            .Select(index => (float)(basis + index * 0.35D + (_random.NextDouble() - 0.5D) * 0.2D))
            .ToArray();
        return new MeasurementReading(position, values, DateTime.UtcNow);
    }
}
