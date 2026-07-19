using System.Windows;
using AmpClean.Presentation.ViewModels;

namespace AmpClean.Presentation;

/// <summary>
/// 代码隐藏仅处理 WPF 生命周期，不包含业务规则。
/// 所有用户操作都通过 ViewModel 命令进入 Application 用例。
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            await vm.InitializeCommand.ExecuteAsync(null);
    }
}
