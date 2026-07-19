namespace AmpClean.Application.Models;

/// <summary>
/// 仪器校准数据集。
/// ReferenceSamples 对应原 RLSAlgorithm 的 RefList（仪器实测特征），
/// StandardSamples 对应 StdList（已知标准目标值）。两个矩阵的行数必须相同。
/// </summary>
public sealed record CalibrationDataset(
    IReadOnlyList<IReadOnlyList<float>> ReferenceSamples,
    IReadOnlyList<IReadOnlyList<float>> StandardSamples);
