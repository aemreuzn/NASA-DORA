using System.Windows;
using NasaAstronautTraining.Services;
using NasaAstronautTraining.ViewModels;

namespace NasaAstronautTraining;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IAuthService authService = new SecureAuthService();
        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel(authService)
        };
        mainWindow.Show();
    }
}
