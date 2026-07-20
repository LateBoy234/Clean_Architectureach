using AmpClean.Application.Models;

namespace AmpClean.Application.Abstractions.Infrastructure;

/// <summary>仪器校准数据端口；模拟实现使用数据库，真实实现可调用厂商 SDK。</summary>
public interface IInstrumentCalibrationStore
{
    Task<IReadOnlyList<IReadOnlyList<float>>> ReadSvdDataAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IReadOnlyList<float>>> ReadStandardDataAsync(
        CancellationToken cancellationToken = default);

    Task WriteCalibrationResultAsync(
        RlsCalculationResult result,
        CancellationToken cancellationToken = default);
}
