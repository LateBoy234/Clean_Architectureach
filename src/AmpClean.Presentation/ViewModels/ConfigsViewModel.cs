using System.Collections.ObjectModel;
using AmpClean.Application.Services;
using AmpClean.Domain.Entities;
using AmpClean.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmpClean.Presentation.ViewModels;

/// <summary>测量配置页面模型，演示完整的 SqlSugar CRUD 数据链路。</summary>
public partial class ConfigsViewModel(MeasureConfigService service) : ObservableObject
{
    [ObservableProperty] private MeasureConfig? _selectedConfig;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _deviceModel = string.Empty;
    [ObservableProperty] private string _motionPathName = string.Empty;
    [ObservableProperty] private InstrumentType _instrumentType = InstrumentType.Spectroradiometer;
    [ObservableProperty] private int _repeatCount = 1;
    [ObservableProperty] private string _message = "可新增或选择一条配置进行编辑";
    [ObservableProperty] private bool _isBusy;

    public ObservableCollection<MeasureConfig> Items { get; } = [];
    public IReadOnlyList<InstrumentType> InstrumentTypes { get; } = Enum.GetValues<InstrumentType>();

    partial void OnSelectedConfigChanged(MeasureConfig? value)
    {
        DeleteCommand.NotifyCanExecuteChanged();
        if (value is null) return;
        Name = value.Name;
        DeviceModel = value.DeviceModel;
        MotionPathName = value.MotionPathName;
        InstrumentType = value.InstrumentType;
        RepeatCount = value.RepeatCount;
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var item in await service.GetAllAsync()) Items.Add(item);
    }

    [RelayCommand]
    private void New()
    {
        SelectedConfig = null;
        Name = DeviceModel = MotionPathName = string.Empty;
        InstrumentType = InstrumentType.Spectroradiometer;
        RepeatCount = 1;
        Message = "正在创建新配置";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var entity = SelectedConfig ?? new MeasureConfig();
            entity.Name = Name.Trim();
            entity.DeviceModel = DeviceModel.Trim();
            entity.MotionPathName = MotionPathName.Trim();
            entity.InstrumentType = InstrumentType;
            entity.RepeatCount = RepeatCount;
            var id = await service.SaveAsync(entity);
            entity.Id = id;
            Message = "配置已保存";
            await LoadAsync();
            SelectedConfig = Items.FirstOrDefault(x => x.Id == id);
        }
        catch (Exception ex) { Message = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync()
    {
        if (SelectedConfig is null) return;
        var deletedName = SelectedConfig.Name;
        await service.DeleteAsync(SelectedConfig.Id);
        New();
        Message = $"已删除：{deletedName}";
        await LoadAsync();
    }

    private bool CanDelete() => SelectedConfig is not null;
}
