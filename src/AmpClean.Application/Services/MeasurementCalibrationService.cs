using AmpClean.Application.Abstractions.Algorithms;
using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Models;

namespace AmpClean.Application.Services;

/// <summary>编排标准值读取、测量结果校准及校准系数写回。</summary>
public sealed class MeasurementCalibrationService(
    IInstrumentCalibrationStore calibrationStore,
    IRlsCalibrationCalculator calculator)
{
    public Task<IReadOnlyList<IReadOnlyList<float>>> ReadSvdDataAsync(
        CancellationToken cancellationToken = default) =>
        calibrationStore.ReadSvdDataAsync(cancellationToken);

    public async Task<RlsCalculationResult> CalibrateAndWriteAsync(
        IReadOnlyList<MeasurementReading> readings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(readings);
        var standards = await calibrationStore.ReadStandardDataAsync(cancellationToken);
        var measured = readings.Select(x => x.Values).ToArray();
        var result = calculator.Calculate(new CalibrationDataset(measured, standards));
        await calibrationStore.WriteCalibrationResultAsync(result, cancellationToken);
        return result;
    }
}
