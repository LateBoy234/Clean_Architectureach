using AmpClean.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmpClean.Presentation.ViewModels;

/// <summary>仪表盘状态只由应用服务提供，页面不直接查询数据库。</summary>
public partial class DashboardViewModel(DashboardService service) : ObservableObject
{
    [ObservableProperty] private long _configCount;
    [ObservableProperty] private long _pathCount;
    [ObservableProperty] private long _reportCount;
    [ObservableProperty] private bool _instrumentConnected;
    [ObservableProperty] private bool _platformConnected;
    [ObservableProperty] private string _statusMessage = "正在读取系统状态…";
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var result = await service.GetSummaryAsync();
            ConfigCount = result.ConfigCount;
            PathCount = result.PathCount;
            ReportCount = result.ReportCount;
            InstrumentConnected = result.InstrumentConnected;
            PlatformConnected = result.PlatformConnected;
            StatusMessage = result.StatusMessage;
        }
        finally { IsBusy = false; }
    }
}
