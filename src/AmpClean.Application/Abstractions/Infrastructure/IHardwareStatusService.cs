namespace AmpClean.Application.Abstractions.Infrastructure;

/// <summary>隔离运动控制卡/仪器厂商 SDK 的端口。</summary>
public interface IHardwareStatusService
{
    Task<HardwareStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}

public sealed record HardwareStatus(bool InstrumentConnected, bool PlatformConnected, string Message);
