namespace AmpClean.Application.Models;

/// <summary>RLS 计算结果；用强类型替代旧项目中的 Dictionary&lt;string, object&gt;。</summary>
public sealed record RlsCalculationResult(
    IReadOnlyList<IReadOnlyList<float>> Coefficients,
    IReadOnlyList<IReadOnlyList<float>> FittedValues,
    IReadOnlyList<double> FinalWeights,
    int Iterations,
    float Mae,
    float Rmse,
    bool Converged);
