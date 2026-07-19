using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Domain.ValueObjects;
using AmpClean.Infrastructure.Persistence;

namespace AmpClean.Infrastructure.Simulation;

/// <summary>
/// 数据库三轴运动控制模拟器。
/// MoveToAsync 等待默认 2 秒模拟平台运动，到位后才把 X/Y/Z 写入数据库。
/// 将来替换真实控制器时，实现同一个 IMotionController，并在 Autofac 中替换注册即可。
/// </summary>
public sealed class DatabaseSimulatedMotionController : IMotionController
{
    private readonly SqlSugarContext _context;
    private readonly TimeSpan _moveInterval;

    public DatabaseSimulatedMotionController(SqlSugarContext context, SimulationOptions options)
    {
        _context = context;
        _moveInterval = TimeSpan.FromMilliseconds(Math.Max(1, options.MoveIntervalMilliseconds));
        var saved = context.Database.Queryable<SimulationAxisState>().InSingle(1);
        CurrentPosition = saved is null
            ? AxisPosition.Origin
            : new AxisPosition(saved.X, saved.Y, saved.Z);
    }

    public AxisPosition CurrentPosition { get; private set; }

    public async Task MoveToAsync(AxisPosition target, CancellationToken cancellationToken = default)
    {
        // Task.Delay 可被停止命令取消；真实控制器应在此等待“运动完成”信号。
        await Task.Delay(_moveInterval, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        CurrentPosition = target;
        await SavePositionAsync(target);
    }

    public Task HomeAsync(CancellationToken cancellationToken = default) =>
        MoveToAsync(AxisPosition.Origin, cancellationToken);

    /// <summary>模拟运动由可取消的 Task.Delay 实现，所以无需额外硬件指令。</summary>
    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private async Task SavePositionAsync(AxisPosition position)
    {
        var state = new SimulationAxisState
        {
            Id = 1,
            X = position.X,
            Y = position.Y,
            Z = position.Z,
            UpdatedAtUtc = DateTime.UtcNow
        };
        await _context.Database.Storageable(state).ExecuteCommandAsync();
    }
}
