namespace AmpClean.Application.Measurement;

/// <summary>状态变化通知，供 WPF、日志或远程监控复用。</summary>
public sealed class MeasurementStateChangedEventArgs(
    MeasurementState state,
    string message,
    int completedCount,
    int totalCount) : EventArgs
{
    public MeasurementState State { get; } = state;
    public string Message { get; } = message;
    public int CompletedCount { get; } = completedCount;
    public int TotalCount { get; } = totalCount;
}
