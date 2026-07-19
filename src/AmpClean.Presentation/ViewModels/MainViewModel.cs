using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AmpClean.Presentation.ViewModels;

/// <summary>主窗口只做页面导航；业务分别下沉到各页面 ViewModel。</summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboard;
    private readonly ConfigsViewModel _configs;
    private readonly PathsViewModel _paths;
    private readonly ReportsViewModel _reports;

    [ObservableProperty] private object? _currentPage;
    [ObservableProperty] private string _currentTitle = "运行概览";

    public MainViewModel(DashboardViewModel dashboard, ConfigsViewModel configs,
        PathsViewModel paths, ReportsViewModel reports)
    {
        _dashboard = dashboard;
        _configs = configs;
        _paths = paths;
        _reports = reports;
        CurrentPage = dashboard;
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; } =
    [
        new("dashboard", "运行概览", PackIconKind.ViewDashboardOutline),
        new("configs", "测量配置", PackIconKind.TuneVariant),
        new("paths", "运动路径", PackIconKind.MapMarkerPath),
        new("reports", "报告管理", PackIconKind.FileChartOutline)
    ];

    [RelayCommand]
    private Task InitializeAsync() => _dashboard.LoadAsync();

    [RelayCommand]
    private async Task NavigateAsync(string? key)
    {
        switch (key)
        {
            case "configs":
                CurrentTitle = "测量配置"; CurrentPage = _configs; await _configs.LoadAsync(); break;
            case "paths":
                CurrentTitle = "运动路径"; CurrentPage = _paths; await _paths.LoadAsync(); break;
            case "reports":
                CurrentTitle = "报告管理"; CurrentPage = _reports; await _reports.LoadAsync(); break;
            default:
                CurrentTitle = "运行概览"; CurrentPage = _dashboard; await _dashboard.LoadAsync(); break;
        }
    }
}
