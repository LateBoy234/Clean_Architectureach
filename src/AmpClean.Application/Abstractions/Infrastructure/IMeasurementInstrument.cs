using AmpClean.Application.Models;
using AmpClean.Domain.ValueObjects;

namespace AmpClean.Application.Abstractions.Infrastructure;

/// <summary>测量仪器端口，真实仪器只需返回统一的 MeasurementReading。</summary>
public interface IMeasurementInstrument
{
    /// <summary>
    /// 读取仪器校准实测矩阵及其配套标准矩阵。
    /// 真实仪器适配器可从设备、校准文件或厂商 SDK 获取这些数据。
    /// </summary>
    Task<CalibrationDataset> ReadCalibrationDataAsync(
        CancellationToken cancellationToken = default);

    Task<MeasurementReading> MeasureAsync(
        AxisPosition position,
        CancellationToken cancellationToken = default);
}
