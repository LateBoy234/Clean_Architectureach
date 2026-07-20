using AmpClean.Application.Algorithms;
using AmpClean.Application.Models;

namespace AmpClean.Application.Tests;

public sealed class RlsCalibrationCalculatorTests
{
    [Fact]
    public void Calculate_WithKnownLinearDataset_ReturnsAccurateFit()
    {
        IReadOnlyList<IReadOnlyList<float>> references =
        [
            [1F, 0F], [1F, 1F], [1F, 2F], [1F, 3F], [1F, 4F]
        ];
        IReadOnlyList<IReadOnlyList<float>> standards =
            references.Select(row => (IReadOnlyList<float>)[2F + 3F * row[1]]).ToArray();

        var result = new RlsCalibrationCalculator()
            .Calculate(new CalibrationDataset(references, standards));

        Assert.True(result.Rmse < 0.0001F);
        Assert.Equal(2F, result.Coefficients[0][0], 3);
        Assert.Equal(3F, result.Coefficients[1][0], 3);
    }

}
