using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaHelloWorld.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Hello, world!";
}
