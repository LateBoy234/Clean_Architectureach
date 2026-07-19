using System.IO;
using System.Text.Json;
using System.Windows;
using AmpClean.Application.Abstractions.Infrastructure;
using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Application.Abstractions.Algorithms;
using AmpClean.Application.Algorithms;
using AmpClean.Application.Services;
using AmpClean.Application.Measurement;
using AmpClean.Infrastructure.Hardware;
using AmpClean.Infrastructure.Persistence;
using AmpClean.Infrastructure.Simulation;
using AmpClean.Presentation.ViewModels;
using Autofac;

namespace AmpClean.Presentation;

public partial class App : System.Windows.Application
{
    private IContainer? _container;

    /// <summary>
    /// App 是 Composition Root：只有最外层知道接口与实现如何装配。
    /// async void 在 WPF 生命周期事件中是合理的，因为框架本身要求 void。
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var errorLogPath = Path.Combine(AppContext.BaseDirectory, "startup-error.log");
        if (File.Exists(errorLogPath)) File.Delete(errorLogPath);
        try
        {
            _container = BuildContainer();
            await _container.Resolve<DatabaseInitializer>().InitializeAsync();
            var window = _container.Resolve<MainWindow>();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            // 即使启动阶段 UI 尚未完成，也保留可诊断信息。
            File.WriteAllText(errorLogPath, ex.ToString());
            MessageBox.Show($"应用启动失败：{ex.Message}", "AMP Clean Architecture",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container?.Dispose();
        base.OnExit(e);
    }

    private static IContainer BuildContainer()
    {
        var settings = LoadSettings();
        var builder = new ContainerBuilder();

        // 基础设施实现：替换数据库或硬件时，仅修改组合根注册。
        builder.RegisterInstance(settings.Database).SingleInstance();
        builder.RegisterInstance(settings.Simulation).SingleInstance();
        builder.RegisterType<SqlSugarContext>().SingleInstance();
        builder.RegisterType<DatabaseInitializer>().SingleInstance();
        builder.RegisterGeneric(typeof(SqlSugarRepository<>)).As(typeof(IRepository<>)).SingleInstance();
        builder.RegisterType<SimulatedHardwareStatusService>().As<IHardwareStatusService>().SingleInstance();
        builder.RegisterType<MeasurementPointProvider>().As<IMeasurementPointProvider>().SingleInstance();
        builder.RegisterType<DatabaseSimulatedMotionController>().As<IMotionController>().SingleInstance();
        builder.RegisterType<FakeMeasurementInstrument>().As<IMeasurementInstrument>().SingleInstance();

        // 应用用例与 ViewModel 全部使用构造函数注入。
        builder.RegisterType<DashboardService>().SingleInstance();
        builder.RegisterType<MeasureConfigService>().SingleInstance();
        builder.RegisterType<MeasurementWorkflow>().SingleInstance();
        builder.RegisterType<RlsCalibrationCalculator>().As<IRlsCalibrationCalculator>().SingleInstance();
        builder.RegisterType<DashboardViewModel>().SingleInstance();
        builder.RegisterType<ConfigsViewModel>().SingleInstance();
        builder.RegisterType<PathsViewModel>().SingleInstance();
        builder.RegisterType<ReportsViewModel>().SingleInstance();
        builder.RegisterType<MeasureViewModel>().SingleInstance();
        builder.RegisterType<MainViewModel>().SingleInstance();
        builder.RegisterType<MainWindow>();
        return builder.Build();
    }

    private static AppSettings LoadSettings()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) return new AppSettings();
        return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppSettings();
    }

    private sealed class AppSettings
    {
        public DatabaseOptions Database { get; init; } = new();
        public SimulationOptions Simulation { get; init; } = new();
    }
}
