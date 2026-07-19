using AmpClean.Application.Algorithms;
using AmpClean.Application.Models;
using AmpClean.Infrastructure.Simulation;

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

    [Fact]
    public async Task FakeInstrumentCalibration_CanBeCalculatedByRls()
    {
        var instrument = new FakeMeasurementInstrument(new SimulationOptions());
        var dataset = await instrument.ReadCalibrationDataAsync();

        var result = new RlsCalibrationCalculator().Calculate(dataset);

        Assert.Equal(12 * 8, dataset.ReferenceSamples.Sum(row => row.Count));
        Assert.Equal(8, result.Coefficients.Count);
        Assert.All(result.Coefficients, row => Assert.Equal(3, row.Count));
        Assert.True(result.Rmse < 0.001F);
    }
}
