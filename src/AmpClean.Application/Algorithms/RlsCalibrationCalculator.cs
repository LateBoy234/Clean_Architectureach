using AmpClean.Application.Abstractions.Algorithms;
using AmpClean.Application.Models;
using MathNet.Numerics.LinearAlgebra;

namespace AmpClean.Application.Algorithms;

/// <summary>
/// 迭代重加权最小二乘（RLS/IRLS）校准算法。
/// 计算过程移植自 AMP/Algorithm/RLSAlgorithm.cs，并将弱类型字典输出改为强类型结果。
/// </summary>
public sealed class RlsCalibrationCalculator : IRlsCalibrationCalculator
{
    public RlsCalculationResult Calculate(
        CalibrationDataset dataset,
        int maxIterations = 100,
        double tolerance = 1e-6,
        IReadOnlyList<float>? initialWeights = null)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        Validate(dataset, initialWeights);

        var sampleCount = dataset.ReferenceSamples.Count;
        var featureCount = dataset.ReferenceSamples[0].Count;
        var targetCount = dataset.StandardSamples[0].Count;

        var referenceMatrix = Matrix<double>.Build.Dense(sampleCount, featureCount,
            (row, column) => dataset.ReferenceSamples[row][column]);
        var standardMatrix = Matrix<double>.Build.Dense(sampleCount, targetCount,
            (row, column) => dataset.StandardSamples[row][column]);

        var currentWeights = initialWeights is null
            ? Vector<double>.Build.Dense(sampleCount, 1D)
            : Vector<double>.Build.Dense(initialWeights.Select(value => (double)value).ToArray());

        var coefficients = WeightedLeastSquares(referenceMatrix, standardMatrix, currentWeights);
        Matrix<double>? lastPrediction = null;
        var iterations = 0;
        var converged = false;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var prediction = referenceMatrix * coefficients;
            if (lastPrediction is not null)
            {
                var predictionChange = (prediction - lastPrediction)
                    .Enumerate().Sum(value => Math.Abs(value));
                if (predictionChange < tolerance)
                {
                    converged = true;
                    break;
                }
            }

            lastPrediction = prediction;
            var residuals = standardMatrix - prediction;
            var newWeights = Vector<double>.Build.Dense(sampleCount);
            for (var row = 0; row < sampleCount; row++)
            {
                var meanResidual = Enumerable.Range(0, targetCount)
                    .Average(column => Math.Abs(residuals[row, column]));
                // 与原算法一致：残差越小，当前样本在下一轮拟合中的权重越高。
                newWeights[row] = 1D / (1e-6D + meanResidual);
            }

            currentWeights = newWeights;
            coefficients = WeightedLeastSquares(referenceMatrix, standardMatrix, currentWeights);
            iterations++;
        }

        var fittedValues = referenceMatrix * coefficients;
        var finalResiduals = standardMatrix - fittedValues;
        var elementCount = sampleCount * targetCount;
        var mae = finalResiduals.Enumerate().Sum(Math.Abs) / elementCount;
        var rmse = Math.Sqrt(finalResiduals.Enumerate().Sum(value => value * value) / elementCount);

        return new RlsCalculationResult(
            ToFloatRows(coefficients),
            ToFloatRows(fittedValues),
            currentWeights.ToArray(),
            iterations,
            (float)mae,
            (float)rmse,
            converged);
    }

    private static Matrix<double> WeightedLeastSquares(
        Matrix<double> features,
        Matrix<double> targets,
        Vector<double> weights)
    {
        var weightMatrix = Matrix<double>.Build.DiagonalOfDiagonalVector(weights);
        var transposedWeighted = features.Transpose() * weightMatrix;
        var normalMatrix = transposedWeighted * features;
        var rightHandSide = transposedWeighted * targets;

        // 保留 AMP 的 QR → SVD → LU 降级策略，兼顾速度和接近奇异矩阵时的稳定性。
        try { return normalMatrix.QR().Solve(rightHandSide); }
        catch
        {
            try { return normalMatrix.Svd().Solve(rightHandSide); }
            catch { return normalMatrix.LU().Solve(rightHandSide); }
        }
    }

    private static void Validate(CalibrationDataset dataset, IReadOnlyList<float>? initialWeights)
    {
        if (dataset.ReferenceSamples.Count == 0 || dataset.StandardSamples.Count == 0)
            throw new ArgumentException("校准数据不能为空。", nameof(dataset));
        if (dataset.ReferenceSamples.Count != dataset.StandardSamples.Count)
            throw new ArgumentException("仪器实测数据与标准数据的样本数量不一致。", nameof(dataset));

        var featureCount = dataset.ReferenceSamples[0].Count;
        var targetCount = dataset.StandardSamples[0].Count;
        if (featureCount == 0 || targetCount == 0)
            throw new ArgumentException("校准矩阵不能包含空行。", nameof(dataset));
        if (dataset.ReferenceSamples.Any(row => row.Count != featureCount))
            throw new ArgumentException("仪器校准数据的特征列数不一致。", nameof(dataset));
        if (dataset.StandardSamples.Any(row => row.Count != targetCount))
            throw new ArgumentException("标准校准数据的目标列数不一致。", nameof(dataset));
        if (initialWeights is not null && initialWeights.Count != dataset.ReferenceSamples.Count)
            throw new ArgumentException("初始权重数量与样本数量不一致。", nameof(initialWeights));
    }

    private static IReadOnlyList<IReadOnlyList<float>> ToFloatRows(Matrix<double> matrix) =>
        Enumerable.Range(0, matrix.RowCount)
            .Select(row => (IReadOnlyList<float>)Enumerable.Range(0, matrix.ColumnCount)
                .Select(column => (float)matrix[row, column]).ToArray())
            .ToArray();
}
