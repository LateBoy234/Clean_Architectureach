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

    /// <summary>
    /// 返回 12 组、每组 8 个特征的模拟校准数据。
    /// StandardSamples 使用固定线性关系生成，便于验证 RLS 能恢复稳定的系数矩阵。
    /// </summary>
    public Task<CalibrationDataset> ReadCalibrationDataAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var references = new List<IReadOnlyList<float>>();
        var standards = new List<IReadOnlyList<float>>();

        for (var sampleIndex = 0; sampleIndex < 12; sampleIndex++)
        {
            var t = sampleIndex + 1F;
            float[] features =
            [
                1F,
                t,
                t * t / 10F,
                MathF.Sin(t),
                MathF.Cos(t),
                sampleIndex % 3,
                sampleIndex % 4,
                sampleIndex % 5
            ];

            // 三个标准目标值可类比 XYZ/Lab；这里使用已知系数构造演示数据。
            float[] targets =
            [
                0.8F * features[0] + 0.3F * features[1] + 0.12F * features[3],
                0.5F * features[0] + 0.2F * features[2] + 0.08F * features[5],
                0.6F * features[0] + 0.15F * features[1] + 0.1F * features[7]
            ];
            references.Add(features);
            standards.Add(targets);
        }

        return Task.FromResult(new CalibrationDataset(references, standards));
    }

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
