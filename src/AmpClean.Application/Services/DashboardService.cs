using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Application.Models;
using AmpClean.Domain.Entities;

namespace AmpClean.Application.Services;

/// <summary>首页查询用例，只负责业务编排，不知道 SqlSugar 和 WPF。</summary>
public sealed class DashboardService(
    IRepository<MeasureConfig> configs,
    IRepository<MotionPath> paths,
    IRepository<MeasurementReport> reports,
    IHardwareStatusService hardware)
{
    public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        // 三个统计查询彼此独立，并行执行可缩短首页加载时间。
        var configTask = configs.CountAsync(cancellationToken);
        var pathTask = paths.CountAsync(cancellationToken);
        var reportTask = reports.CountAsync(cancellationToken);
        var hardwareTask = hardware.GetStatusAsync(cancellationToken);
        await Task.WhenAll(configTask, pathTask, reportTask, hardwareTask);

        var status = await hardwareTask;
        return new DashboardSummary(
            await configTask,
            await pathTask,
            await reportTask,
            status.InstrumentConnected,
            status.PlatformConnected,
            status.Message);
    }
}
