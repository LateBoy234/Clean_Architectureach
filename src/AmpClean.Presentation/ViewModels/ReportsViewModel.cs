using System.Collections.ObjectModel;
using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmpClean.Presentation.ViewModels;

/// <summary>报告查询页面模型；Word/Excel 导出可作为基础设施适配器继续添加。</summary>
public sealed partial class ReportsViewModel(IRepository<MeasurementReport> repository) : ObservableObject
{
    public ObservableCollection<MeasurementReport> Items { get; } = [];

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var item in await repository.GetAllAsync()) Items.Add(item);
    }
}
