namespace AmpClean.Application.Models;

public sealed class CalibrationCompletedEventArgs(RlsCalculationResult result) : EventArgs
{
    public RlsCalculationResult Result { get; } = result;
}
