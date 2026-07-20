using System.Text.Json;
using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Models;

namespace AmpClean.Infrastructure.Persistence;

public sealed class DatabaseInstrumentCalibrationStore(SqlSugarContext context)
    : IInstrumentCalibrationStore
{
    public async Task<IReadOnlyList<IReadOnlyList<float>>> ReadSvdDataAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var record = await GetRecordAsync();
        return Deserialize(record.SvdDataJson, "仪器 SVD");
    }

    public async Task<IReadOnlyList<IReadOnlyList<float>>> ReadStandardDataAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var record = await GetRecordAsync();
        return Deserialize(record.StandardDataJson, "标准");
    }

    public async Task WriteCalibrationResultAsync(
        RlsCalculationResult result,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var record = await GetRecordAsync();
        record.CoefficientsJson = JsonSerializer.Serialize(result.Coefficients);
        record.Iterations = result.Iterations;
        record.Mae = result.Mae;
        record.Rmse = result.Rmse;
        record.CalibratedAtUtc = DateTime.UtcNow;
        record.UpdatedAtUtc = DateTime.UtcNow;
        await context.Database.Updateable(record).ExecuteCommandAsync();
    }

    private async Task<InstrumentCalibrationRecord> GetRecordAsync() =>
        await context.Database.Queryable<InstrumentCalibrationRecord>().FirstAsync()
        ?? throw new InvalidOperationException("数据库中没有模拟仪器校准数据。");

    private static IReadOnlyList<IReadOnlyList<float>> Deserialize(string json, string name)
    {
        var rows = JsonSerializer.Deserialize<float[][]>(json);
        if (rows is null || rows.Length == 0)
            throw new InvalidOperationException($"{name}数据为空。");
        return rows;
    }
}
