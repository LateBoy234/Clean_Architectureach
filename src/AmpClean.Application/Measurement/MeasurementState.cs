namespace AmpClean.Application.Measurement;

/// <summary>自动测量流程的有限状态集合。</summary>
public enum MeasurementState
{
    Idle,
    LoadingPoints,
    Moving,
    Measuring,
    Calibrating,
    Paused,
    Completed,
    Homing,
    Faulted
}
