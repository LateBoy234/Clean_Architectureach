using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using AmpClean.Application.Measurement;
using AmpClean.Application.Abstractions.Algorithms;
using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Presentation.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmpClean.Presentation.ViewModels;

/// <summary>
/// 测量页面 ViewModel。
/// 四个命令只向 MeasurementWorkflow 发送意图，不直接操作 SqlSugar 或硬件，
/// 因此将来切换真实运动控制和真实仪器时，此处无需改变流程代码。
/// </summary>
public partial class MeasureViewModel : ObservableObject
{
    private readonly MeasurementWorkflow _workflow;
    private readonly IMeasurementInstrument _instrument;
    private readonly IRlsCalibrationCalculator _calculator;
    private bool _calibrationReady;

    public MeasureViewModel(
        MeasurementWorkflow workflow,
        IMeasurementInstrument instrument,
        IRlsCalibrationCalculator calculator)
    {
        _workflow = workflow;
        _instrument = instrument;
        _calculator = calculator;
        _workflow.StateChanged += OnStateChanged;
        _workflow.SampleCompleted += OnSampleCompleted;
    }

    /// <summary>每完成一个点位，状态机便向此集合追加一行假测量数据。</summary>
    [ObservableProperty]
    private ObservableCollection<MeasurementData> _measurementData = [];

    /// <summary>仪器返回的校准实测矩阵，按行展平后显示在“仪器数据”区域。</summary>
    [ObservableProperty] private ObservableCollection<float> _readDataList = [];

    /// <summary>RLS 输出的回归系数矩阵，按行展平后显示在“计算数据”区域。</summary>
    [ObservableProperty] private ObservableCollection<float> _resultDataList = [];

    [ObservableProperty] private int _colsNumber = 8;
    [ObservableProperty] private int _resultColsNumber = 3;
    [ObservableProperty] private string _calibrationStatus = "尚未读取仪器校准数据";
    [ObservableProperty] private bool _isCalibrationLoading;

    [ObservableProperty] private Brush _remindColor = Brushes.Gray;
    [ObservableProperty] private string _stateText = "待机";
    [ObservableProperty] private string _progressText = "0 / 0";
    [ObservableProperty] private double _currentX;
    [ObservableProperty] private double _currentY;
    [ObservableProperty] private double _currentZ;

    /// <summary>
    /// 进入测量页面时调用：读取仪器校准数据、填充 ReadDataList，并立即执行 RLS。
    /// 方法属于页面加载用例，与自动测量状态机彼此独立。
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsCalibrationLoading || _workflow.IsRunning) return;
        try
        {
            IsCalibrationLoading = true;
            _calibrationReady = false;
            CalibrationStatus = "正在读取仪器校准数据…";
            ReadDataList.Clear();
            ResultDataList.Clear();

            var dataset = await _instrument.ReadCalibrationDataAsync(cancellationToken);
            ColsNumber = dataset.ReferenceSamples.FirstOrDefault()?.Count ?? 1;
            foreach (var value in dataset.ReferenceSamples.SelectMany(row => row))
                ReadDataList.Add(value);

            CalibrationStatus = "校准数据读取完成，正在进行 RLS 计算…";
            var result = await Task.Run(() => _calculator.Calculate(dataset), cancellationToken);
            ResultColsNumber = result.Coefficients.FirstOrDefault()?.Count ?? 1;
            foreach (var value in result.Coefficients.SelectMany(row => row))
                ResultDataList.Add(value);

            _calibrationReady = true;
            CalibrationStatus =
                $"RLS 完成：迭代 {result.Iterations} 次，MAE={result.Mae:F6}，RMSE={result.Rmse:F6}";
        }
        catch (OperationCanceledException)
        {
            CalibrationStatus = "读取校准数据已取消。";
        }
        catch (Exception ex)
        {
            CalibrationStatus = $"校准数据或 RLS 计算失败：{ex.Message}";
        }
        finally
        {
            IsCalibrationLoading = false;
            AutoMeasureCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    /// 自动测量：清空上一轮 UI 数据，从数据库第一个启用点位重新开始。
    /// RelayCommand 会生成 AutoMeasureCommand。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task AutoMeasureAsync()
    {
        MeasurementData.Clear();
        await _workflow.StartAsync();
    }

    /// <summary>继续测量：仅在 Paused 状态可用，从尚未完成的点位继续。</summary>
    [RelayCommand(CanExecute = nameof(CanContinue))]
    private Task ContinueMeasureAsync() => _workflow.ContinueAsync();

    /// <summary>
    /// 停止测量：通过 CancellationToken 取消当前移动或采样，
    /// 状态机保留点位游标，以便之后继续。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStop))]
    private Task StopMeasureAsync() => _workflow.StopAsync();

    /// <summary>回到原点：先安全停止自动流程，再执行三轴回零。</summary>
    [RelayCommand(CanExecute = nameof(CanReturnToOrigin))]
    private Task ReturnToOriginAsync() => _workflow.ReturnToOriginAsync();

    private bool CanStart() => _calibrationReady && !_workflow.IsRunning
        && _workflow.State != MeasurementState.Homing;
    private bool CanContinue() => _workflow.State == MeasurementState.Paused;
    private bool CanStop() => _workflow.IsRunning;
    private bool CanReturnToOrigin() => _workflow.State != MeasurementState.Homing;

    private void OnStateChanged(object? sender, MeasurementStateChangedEventArgs e)
    {
        RunOnUiThread(() =>
        {
            StateText = e.Message;
            ProgressText = $"{e.CompletedCount} / {e.TotalCount}";
            RemindColor = e.State switch
            {
                MeasurementState.Completed => Brushes.SeaGreen,
                MeasurementState.Faulted => Brushes.IndianRed,
                MeasurementState.Paused => Brushes.DarkOrange,
                MeasurementState.Moving or MeasurementState.Measuring => Brushes.RoyalBlue,
                _ => Brushes.Gray
            };

            // 状态变化后重新计算四个按钮的可用性。
            AutoMeasureCommand.NotifyCanExecuteChanged();
            ContinueMeasureCommand.NotifyCanExecuteChanged();
            StopMeasureCommand.NotifyCanExecuteChanged();
            ReturnToOriginCommand.NotifyCanExecuteChanged();
        });
    }

    private void OnSampleCompleted(object? sender, MeasurementSampleEventArgs e)
    {
        RunOnUiThread(() =>
        {
            var sample = e.Sample;
            CurrentX = sample.Reading.Position.X;
            CurrentY = sample.Reading.Position.Y;
            CurrentZ = sample.Reading.Position.Z;
            MeasurementData.Add(new MeasurementData
            {
                Index = sample.Point.Sequence,
                PointName = sample.Point.Name,
                X = sample.Reading.Position.X,
                Y = sample.Reading.Position.Y,
                Z = sample.Reading.Position.Z,
                MeasurementItems = sample.Reading.Values.ToList(),
                MeasuredAt = sample.Reading.MeasuredAtUtc.ToLocalTime()
            });
        });
    }

    private static void RunOnUiThread(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action);
    }
}
