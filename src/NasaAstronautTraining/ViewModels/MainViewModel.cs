using CommunityToolkit.Mvvm.ComponentModel;
using NasaAstronautTraining.Models;
using NasaAstronautTraining.Services;

namespace NasaAstronautTraining.ViewModels;

/// <summary>
/// Root ViewModel:
/// 1. LoginViewModel (Giriş Ekranı)
/// 2. LaunchpadViewModel (Karşılama &amp; Biniş Kartı)
/// 3. MissionControlViewModel (KOMUTA, AKADEMİ, HANGAR, KÜNYE ve AYARLAR sekmelerinin tamamını tek ekranda barındıran Ana Komuta Merkezi)
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableObject _currentScene = null!;

    public MainViewModel(IAuthService authService)
    {
        _authService = authService;
        ShowLogin();
    }

    private void ShowLogin()
    {
        CurrentScene = new LoginViewModel(_authService, OnLoginSuccess);
    }

    private void OnLoginSuccess(AstronautSession session)
    {
        CurrentScene = new LaunchpadViewModel(session, OnStartMission);
    }

    private void OnStartMission(AstronautSession session, string role)
    {
        CurrentScene = new MissionControlViewModel(session, role, ShowLogin);
    }
}
