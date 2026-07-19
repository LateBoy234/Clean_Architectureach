using System.Collections.ObjectModel;
using AmpClean.Application.Abstractions.Persistence;
using AmpClean.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AmpClean.Presentation.ViewModels;

/// <summary>运动路径只读列表；硬件路径编辑器可在此边界上继续扩展。</summary>
public sealed partial class PathsViewModel(IRepository<MotionPath> repository) : ObservableObject
{
    public ObservableCollection<MotionPath> Items { get; } = [];

    public async Task LoadAsync()
    {
        Items.Clear();
        foreach (var item in await repository.GetAllAsync()) Items.Add(item);
    }
}
