namespace AmpClean.Infrastructure.Simulation;

/// <summary>模拟设备参数，可在 appsettings.json 中调整。</summary>
public sealed class SimulationOptions
{
    /// <summary>每次移动到下一个点位所需的模拟时间，默认 2 秒。</summary>
    public int MoveIntervalMilliseconds { get; init; } = 2000;

    /// <summary>模拟仪器单次采样时间；默认 0，确保点位启动间隔约为 2 秒。</summary>
    public int MeasureDurationMilliseconds { get; init; }
}
