using AmpClean.Application.Models;

namespace AmpClean.Application.Abstractions.Algorithms;

/// <summary>校准算法端口，方便测试并允许以后切换算法版本。</summary>
public interface IRlsCalibrationCalculator
{
    RlsCalculationResult Calculate(
        CalibrationDataset dataset,
        int maxIterations = 100,
        double tolerance = 1e-6,
        IReadOnlyList<float>? initialWeights = null);
}
