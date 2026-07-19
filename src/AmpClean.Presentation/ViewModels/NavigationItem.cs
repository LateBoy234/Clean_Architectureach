using MaterialDesignThemes.Wpf;

namespace AmpClean.Presentation.ViewModels;

/// <summary>侧边导航的纯展示模型。</summary>
public sealed record NavigationItem(string Key, string Title, PackIconKind Icon);
