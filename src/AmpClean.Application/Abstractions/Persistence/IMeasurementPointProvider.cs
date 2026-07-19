using AmpClean.Domain.Entities;

namespace AmpClean.Application.Abstractions.Persistence;

/// <summary>按执行顺序从数据库读取启用的测量点位。</summary>
public interface IMeasurementPointProvider
{
    Task<IReadOnlyList<MeasurementPoint>> GetEnabledPointsAsync(
        CancellationToken cancellationToken = default);
}
