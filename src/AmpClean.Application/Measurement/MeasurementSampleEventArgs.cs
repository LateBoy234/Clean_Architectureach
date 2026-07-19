using AmpClean.Application.Models;

namespace AmpClean.Application.Measurement;

public sealed class MeasurementSampleEventArgs(MeasurementSample sample) : EventArgs
{
    public MeasurementSample Sample { get; } = sample;
}
