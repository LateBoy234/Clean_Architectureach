namespace AmpClean.Application.Measurement;

/// <summary>自动测量流程的有限状态集合。</summary>
public enum MeasurementState
{
    Idle,
    LoadingPoints,
    Moving,
    Measuring,
    Paused,
    Completed,
    Homing,
    Faulted
}
