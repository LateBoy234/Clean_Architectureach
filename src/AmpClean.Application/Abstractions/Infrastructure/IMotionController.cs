using AmpClean.Domain.ValueObjects;

namespace AmpClean.Application.Abstractions.Infrastructure;

/// <summary>
/// 三轴运动控制端口。应用层只依赖毫米坐标和异步动作，
/// 后续接入真实运动卡时由 Infrastructure 提供新的实现。
/// </summary>
public interface IMotionController
{
    AxisPosition CurrentPosition { get; }

    /// <summary>移动到绝对坐标；完成返回表示三轴均已到位。</summary>
    Task MoveToAsync(AxisPosition target, CancellationToken cancellationToken = default);

    /// <summary>
    /// 主动停止当前运动。真实控制卡通常需要发送减速停止或急停指令，
    /// 不能只依赖托管 CancellationToken。
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>执行安全回原点动作。</summary>
    Task HomeAsync(CancellationToken cancellationToken = default);
}
