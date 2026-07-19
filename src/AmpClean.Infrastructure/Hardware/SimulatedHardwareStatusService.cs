using AmpClean.Application.Abstractions.Infrastructure;

namespace AmpClean.Infrastructure.Hardware;

/// <summary>
/// 可运行骨架使用的模拟硬件适配器。接入 AMP 的 MultiCard/仪器 DLL 时，
/// 新建另一个 IHardwareStatusService 实现即可，不需要修改页面和用例。
/// </summary>
public sealed class SimulatedHardwareStatusService : IHardwareStatusService
{
    public Task<HardwareStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new HardwareStatus(true, true, "模拟仪器与运动平台运行正常"));
    }
}
