using AmpClean.Application.Models;
using AmpClean.Domain.ValueObjects;

namespace AmpClean.Application.Abstractions.Infrastructure;

/// <summary>测量仪器端口，真实仪器只需返回统一的 MeasurementReading。</summary>
public interface IMeasurementInstrument
{
    Task<MeasurementReading> MeasureAsync(
        AxisPosition position,
        CancellationToken cancellationToken = default);
}
