using AmpClean.Domain.ValueObjects;

namespace AmpClean.Application.Models;

/// <summary>一次仪器采样的统一输出，Values 可承载 XYZ、Lab 或光谱数据。</summary>
public sealed record MeasurementReading(
    AxisPosition Position,
    IReadOnlyList<float> Values,
    DateTime MeasuredAtUtc);
