using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Application.Measurement;
using AmpClean.Application.Models;
using AmpClean.Domain.Entities;
using AmpClean.Domain.ValueObjects;

namespace AmpClean.Application.Tests;

/// <summary>状态机测试使用内存假对象，不依赖 WPF、SqlSugar 或真实硬件。</summary>
public sealed class MeasurementWorkflowTests
{
    [Fact]
    public async Task StartAsync_MeasuresEveryPoint_AndCompletes()
    {
        var points = new[]
        {
            new MeasurementPoint { Sequence = 1, X = 1, Y = 2, Z = 3 },
            new MeasurementPoint { Sequence = 2, X = 4, Y = 5, Z = 6 }
        };
        var workflow = new MeasurementWorkflow(
            new StubPointProvider(points), new ImmediateMotionController(), new StubInstrument());
        var samples = new List<MeasurementSample>();
        workflow.SampleCompleted += (_, e) => samples.Add(e.Sample);

        await workflow.StartAsync();

        Assert.Equal(MeasurementState.Completed, workflow.State);
        Assert.Equal(2, samples.Count);
        Assert.Equal(2, samples[1].Point.Sequence);
    }

    [Fact]
    public async Task StopThenContinue_ResumesCurrentPoint()
    {
        var motion = new FirstMoveCanBeCancelledController();
        var workflow = new MeasurementWorkflow(
            new StubPointProvider([new MeasurementPoint { Sequence = 1 }]),
            motion, new StubInstrument());

        var startTask = workflow.StartAsync();
        await motion.FirstMoveStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await workflow.StopAsync();
        await startTask;

        Assert.Equal(MeasurementState.Paused, workflow.State);
        await workflow.ContinueAsync();
        Assert.Equal(MeasurementState.Completed, workflow.State);
    }

    private sealed class StubPointProvider(IReadOnlyList<MeasurementPoint> points)
        : IMeasurementPointProvider
    {
        public Task<IReadOnlyList<MeasurementPoint>> GetEnabledPointsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(points);
    }

    private sealed class ImmediateMotionController : IMotionController
    {
        public AxisPosition CurrentPosition { get; private set; }
        public Task MoveToAsync(AxisPosition target, CancellationToken cancellationToken = default)
        { CurrentPosition = target; return Task.CompletedTask; }
        public Task HomeAsync(CancellationToken cancellationToken = default)
        { CurrentPosition = AxisPosition.Origin; return Task.CompletedTask; }
        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FirstMoveCanBeCancelledController : IMotionController
    {
        private int _calls;
        public TaskCompletionSource FirstMoveStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public AxisPosition CurrentPosition { get; private set; }

        public async Task MoveToAsync(AxisPosition target, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _calls) == 1)
            {
                FirstMoveStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            CurrentPosition = target;
        }

        public Task HomeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubInstrument : IMeasurementInstrument
    {
        public Task<CalibrationDataset> ReadCalibrationDataAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new CalibrationDataset([[1F]], [[1F]]));

        public Task<MeasurementReading> MeasureAsync(AxisPosition position,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new MeasurementReading(position, [1F, 2F, 3F], DateTime.UtcNow));
    }
}
