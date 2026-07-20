using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Application.Models;
using AmpClean.Application.Services;
using AmpClean.Domain.Entities;

namespace AmpClean.Application.Measurement;

/// <summary>
/// 自动测量状态机：加载点位 → 移动 → 测量 → 下一点 → 完成。
/// 它不引用 WPF、SqlSugar 或厂商 DLL，因此模拟设备和真实设备遵循同一流程。
/// </summary>
public sealed class MeasurementWorkflow(
    IMeasurementPointProvider pointProvider,
    IMotionController motionController,
    IMeasurementInstrument instrument,
    MeasurementCalibrationService calibrationService)
{
    private readonly SemaphoreSlim _transitionLock = new(1, 1);
    private IReadOnlyList<MeasurementPoint> _points = [];
    private CancellationTokenSource? _runCancellation;
    private Task? _runningTask;
    private int _nextPointIndex;
    private readonly List<MeasurementReading> _readings = [];

    public MeasurementState State { get; private set; } = MeasurementState.Idle;
    public bool IsRunning => State is MeasurementState.LoadingPoints
        or MeasurementState.Moving or MeasurementState.Measuring or MeasurementState.Calibrating;

    public event EventHandler<MeasurementStateChangedEventArgs>? StateChanged;
    public event EventHandler<MeasurementSampleEventArgs>? SampleCompleted;
    public event EventHandler<CalibrationCompletedEventArgs>? CalibrationCompleted;

    /// <summary>从第一个数据库点位开始一轮全新的自动测量。</summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _transitionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsRunning) return;

            ChangeState(MeasurementState.LoadingPoints, "正在从数据库加载测量点位…");
            try
            {
                _points = await pointProvider.GetEnabledPointsAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                ChangeState(MeasurementState.Paused, "加载点位已取消。");
                return;
            }
            catch (Exception ex)
            {
                ChangeState(MeasurementState.Faulted, $"加载数据库点位失败：{ex.Message}");
                return;
            }
            if (_points.Count == 0)
            {
                ChangeState(MeasurementState.Faulted, "数据库中没有可用的测量点位。");
                return;
            }

            _nextPointIndex = 0;
            _readings.Clear();
            _runCancellation?.Dispose();
            _runCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runningTask = RunLoopAsync(_runCancellation.Token);
        }
        finally { _transitionLock.Release(); }

        await _runningTask;
    }

    /// <summary>从停止时尚未完成的点位继续，不重复已经产生的数据。</summary>
    public async Task ContinueAsync(CancellationToken cancellationToken = default)
    {
        await _transitionLock.WaitAsync(cancellationToken);
        try
        {
            if (State != MeasurementState.Paused || _nextPointIndex >= _points.Count) return;
            _runCancellation?.Dispose();
            _runCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _runningTask = RunLoopAsync(_runCancellation.Token);
        }
        finally { _transitionLock.Release(); }

        await _runningTask;
    }

    /// <summary>
    /// 请求停止。CancellationToken 会继续传给真实运动控制器/仪器，
    /// 适配器应在安全边界停止当前动作。
    /// </summary>
    public async Task StopAsync()
    {
        Task? runningTask;
        await _transitionLock.WaitAsync();
        try
        {
            if (!IsRunning) return;
            _runCancellation?.Cancel();
            runningTask = _runningTask;
        }
        finally { _transitionLock.Release(); }

        // CancellationToken 通知托管异步流程；StopAsync 负责向真实控制卡发停止指令。
        Exception? stopError = null;
        try
        {
            await motionController.StopAsync();
        }
        catch (Exception ex)
        {
            stopError = ex;
        }
        if (runningTask is not null)
            await runningTask;
        if (stopError is not null)
            ChangeState(MeasurementState.Faulted, $"停止运动控制器失败：{stopError.Message}");
    }

    /// <summary>先停止自动流程，再调用控制器执行回原点。</summary>
    public async Task ReturnToOriginAsync(CancellationToken cancellationToken = default)
    {
        await StopAsync();
        ChangeState(MeasurementState.Homing, "三轴平台正在回原点…");
        try
        {
            await motionController.HomeAsync(cancellationToken);
            _nextPointIndex = 0;
            ChangeState(MeasurementState.Idle, "平台已回到机械原点。");
        }
        catch (Exception ex)
        {
            ChangeState(MeasurementState.Faulted, $"回原点失败：{ex.Message}");
        }
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_nextPointIndex < _points.Count)
            {
                var point = _points[_nextPointIndex];
                ChangeState(MeasurementState.Moving,
                    $"正在移动到点位 {point.Sequence}：X={point.X:F2}, Y={point.Y:F2}, Z={point.Z:F2}");
                await motionController.MoveToAsync(point.ToPosition(), cancellationToken);

                ChangeState(MeasurementState.Measuring, $"点位 {point.Sequence} 已到位，正在采集数据…");
                var reading = await instrument.MeasureAsync(point.ToPosition(), cancellationToken);
                _readings.Add(reading);
                SampleCompleted?.Invoke(this,
                    new MeasurementSampleEventArgs(new MeasurementSample(point, reading)));

                // 只有运动和测量都成功后才推进游标，停止后会安全地重试当前点。
                _nextPointIndex++;
            }

            ChangeState(MeasurementState.Calibrating, "测量完成，正在使用标准数据校准并写入仪器…");
            var calibration = await calibrationService.CalibrateAndWriteAsync(_readings, cancellationToken);
            CalibrationCompleted?.Invoke(this, new CalibrationCompletedEventArgs(calibration));
            ChangeState(MeasurementState.Completed, "全部测量点位已完成，校准结果已写入仪器。");
        }
        catch (OperationCanceledException)
        {
            ChangeState(MeasurementState.Paused,
                $"测量已停止，进度 {_nextPointIndex}/{_points.Count}，可继续测量。");
        }
        catch (Exception ex)
        {
            ChangeState(MeasurementState.Faulted, $"测量流程失败：{ex.Message}");
        }
    }

    private void ChangeState(MeasurementState state, string message)
    {
        State = state;
        StateChanged?.Invoke(this,
            new MeasurementStateChangedEventArgs(state, message, _nextPointIndex, _points.Count));
    }
}
