using AmpClean.Domain.Common;

namespace AmpClean.Infrastructure.Persistence;

/// <summary>模拟仪器的校准存储；矩阵使用 JSON 保存，便于替换成真实设备协议。</summary>
public sealed class InstrumentCalibrationRecord : Entity
{
    public string SvdDataJson { get; set; } = "[]";
    public string StandardDataJson { get; set; } = "[]";
    public string CoefficientsJson { get; set; } = "[]";
    public int Iterations { get; set; }
    public float Mae { get; set; }
    public float Rmse { get; set; }
    public DateTime CalibratedAtUtc { get; set; }
}
