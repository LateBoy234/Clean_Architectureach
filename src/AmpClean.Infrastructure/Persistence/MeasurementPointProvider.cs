using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Domain.Entities;

namespace AmpClean.Infrastructure.Persistence;

/// <summary>SqlSugar 点位查询实现，确保状态机始终按 Sequence 顺序运行。</summary>
public sealed class MeasurementPointProvider(SqlSugarContext context) : IMeasurementPointProvider
{
    public async Task<IReadOnlyList<MeasurementPoint>> GetEnabledPointsAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await context.Database.Queryable<MeasurementPoint>()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Sequence)
            .ToListAsync();
    }
}
