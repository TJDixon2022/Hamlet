using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HamManager.App.ViewModels;
using HamManager.App.Views;
using HamManager.RadioEngine.Rig;

namespace HamManager.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // FakeRig until the CI-V IRig lands (HM-DEC-003). Swapping the
            // implementation here is the only change the UI will feel.
            IRig rig = new FakeRig();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(rig),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
